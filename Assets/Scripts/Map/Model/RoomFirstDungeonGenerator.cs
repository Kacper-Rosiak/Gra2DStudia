using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [Header("Dungeon Settings")]
    [SerializeField] private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField] private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField][Range(0, 10)] private int offset = 1;
    [SerializeField] private bool randomWalkRooms = false;

    [Header("PCG - Spawning System")]
    [SerializeField] private List<GameObject> enemyPrefabsAscending;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private Transform entityParent;
    [SerializeField] private int maxEnemiesPerDungeon = 10;
    [SerializeField] private int maxChestsPerDungeon = 2;
    [SerializeField] private int minEnemyDistanceFromWall = 2; // NOWY ARGUMENT: Odleg³oœæ wroga od œciany

    protected override void RunProceduralGeneration()
    {
        // Gwarancja czystej sceny przed now¹ generacj¹
        if (entityParent != null)
        {
            for (int i = entityParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(entityParent.GetChild(i).gameObject);
            }
        }
        CreateRooms();
    }

    private void CreateRooms()
    {
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
            new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        if (randomWalkRooms) floor = CreateRoomsRandomly(roomsList);
        else floor = CreateSimpleRooms(roomsList);

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList) roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));

        // Przeniesienie gracza do pierwszego wygenerowanego pokoju
        if (roomCenters.Count > 0)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            // JeÅ›li gracza nie ma, sprÃ³buj go zespawnowaÄ‡ TERAZ przez spawner
            if (player == null && PlayerSpawner.Instance != null)
            {
                player = PlayerSpawner.Instance.SpawnPlayer();
            }

            if (player != null)
            {
                Vector3 newPos = new Vector3(roomCenters[0].x, roomCenters[0].y, 0);
                
                // UÅ¼ywamy Rigidbody2D do teleportacji, jeÅ›li istnieje, aby nie kÅ‚Ã³ciÄ‡ siÄ™ z silnikiem fizyki
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position = newPos;
                }
                player.transform.position = newPos;
                
                Debug.Log($"Generator: Teleportowano gracza do Å›rodka pokoju: {newPos}");

                // Ponowne przypisanie kamery po teleportacji
                if (PlayerSpawner.Instance != null)
                {
                    PlayerSpawner.Instance.AssignCamera(player);
                }
            }
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);
<<<<<<< Updated upstream
        tilemapVisualizer.CompressAllBounds();
=======

        PopulateRooms(roomsList, floor, corridors);
>>>>>>> Stashed changes
    }

    private void PopulateRooms(List<BoundsInt> rooms, HashSet<Vector2Int> floor, HashSet<Vector2Int> corridors)
    {
        Vector2Int startPos = (Vector2Int)startPosition;
        float maxDistance = 0f;
        int totalEnemiesSpawned = 0;
        int totalChestsSpawned = 0;

        foreach (var room in rooms)
        {
            float dist = Vector2.Distance(startPos, (Vector2Int)Vector3Int.RoundToInt(room.center));
            if (dist > maxDistance) maxDistance = dist;
        }

        foreach (var room in rooms)
        {
            Vector2Int idealCenter = (Vector2Int)Vector3Int.RoundToInt(room.center);
            float distanceFromStart = Vector2.Distance(startPos, idealCenter);

            if (distanceFromStart < 2f) continue;

            float difficultyFactor = maxDistance > 0 ? (distanceFromStart / maxDistance) : 0f;

            List<Vector2Int> safePoints = GetSafeSpawnPoints(room, floor, corridors);
            if (safePoints.Count == 0) continue;

            // 1. LOGIKA SKRZYÑ (Max 2, na œrodku pokoju)
            if (totalChestsSpawned < maxChestsPerDungeon)
            {
                if (Random.value < 0.6f)
                {
                    Vector2Int bestChestPos = FindBestPositionNearCenter(idealCenter, safePoints);
                    SpawnChest(bestChestPos, difficultyFactor);
                    totalChestsSpawned++;
                    safePoints.Remove(bestChestPos);
                }
            }

            // 2. LOGIKA PRZECIWNIKÓW (Odleg³oœæ od œciany, max 1 w pokoju)
            if (totalEnemiesSpawned < maxEnemiesPerDungeon)
            {
                // Filtrujemy bezpieczne punkty przez nasz¹ now¹ metodê dystansu od œcian
                List<Vector2Int> enemySafePoints = GetPointsAwayFromWall(safePoints, floor, minEnemyDistanceFromWall);

                if (enemySafePoints.Count > 0)
                {
                    Vector2Int pos = GetAndRemoveRandomPoint(enemySafePoints);
                    SpawnEnemy(pos, difficultyFactor);
                    totalEnemiesSpawned++;
                }
            }
        }
    }

    // --- NOWA METODA: Odrzucanie punktów zbyt blisko œcian ---
    private List<Vector2Int> GetPointsAwayFromWall(List<Vector2Int> safePoints, HashSet<Vector2Int> floor, int requiredDistance)
    {
        List<Vector2Int> filteredPoints = new List<Vector2Int>();

        foreach (Vector2Int pos in safePoints)
        {
            bool isFarEnough = true;

            // Sprawdzamy kwadrat (np. 5x5 dla dystansu 2) wokó³ punktu spawnu
            for (int x = -requiredDistance; x <= requiredDistance; x++)
            {
                for (int y = -requiredDistance; y <= requiredDistance; y++)
                {
                    // Jeœli w promieniu 2 kratek brakuje pod³ogi, znaczy ¿e natrafiliœmy na œcianê (lub pustkê)
                    if (!floor.Contains(pos + new Vector2Int(x, y)))
                    {
                        isFarEnough = false;
                        break;
                    }
                }
                if (!isFarEnough) break;
            }

            if (isFarEnough)
            {
                filteredPoints.Add(pos);
            }
        }

        return filteredPoints;
    }

    private Vector2Int FindBestPositionNearCenter(Vector2Int idealCenter, List<Vector2Int> safePoints)
    {
        Vector2Int bestPos = safePoints[0];
        float minD = float.MaxValue;
        foreach (var sp in safePoints)
        {
            float d = Vector2.Distance(idealCenter, sp);
            if (d < minD)
            {
                minD = d;
                bestPos = sp;
            }
        }
        return bestPos;
    }

    private List<Vector2Int> GetSafeSpawnPoints(BoundsInt room, HashSet<Vector2Int> floor, HashSet<Vector2Int> corridors)
    {
        List<Vector2Int> safePoints = new List<Vector2Int>();
        foreach (Vector2Int pos in floor)
        {
            if (pos.x >= room.xMin + offset && pos.x <= room.xMax - offset &&
                pos.y >= room.yMin + offset && pos.y <= room.yMax - offset)
            {
                if (!corridors.Contains(pos)) safePoints.Add(pos);
            }
        }
        return safePoints;
    }

    private void SpawnChest(Vector2Int position, float difficulty)
    {
        if (chestPrefab == null) return;
        GameObject chestObj = Instantiate(chestPrefab, new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity, entityParent);
        Chest chestComp = chestObj.GetComponent<Chest>();
        if (chestComp != null)
        {
            if (difficulty >= 0.70f) chestComp.chestRarity = LootRarity.Epic;
            else if (difficulty >= 0.35f) chestComp.chestRarity = LootRarity.Rare;
            else chestComp.chestRarity = LootRarity.Common;
        }
    }

    private void SpawnEnemy(Vector2Int position, float difficulty)
    {
        if (enemyPrefabsAscending == null || enemyPrefabsAscending.Count == 0) return;
        int maxIndex = enemyPrefabsAscending.Count - 1;
        int selectedIndex = Mathf.Clamp(Mathf.RoundToInt(difficulty * maxIndex), 0, maxIndex);
        Instantiate(enemyPrefabsAscending[selectedIndex], new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity, entityParent);
    }

    private Vector2Int GetAndRemoveRandomPoint(List<Vector2Int> points)
    {
        int index = Random.Range(0, points.Count);
        Vector2Int point = points[index];
        points.RemoveAt(index);
        return point;
    }

    // --- Metody BSP i Random Walk z bazowego kodu ---
    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) && position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
<<<<<<< Updated upstream

            // Generujemy ï¿½cieï¿½kï¿½ miï¿½dzy pokojami
=======
>>>>>>> Stashed changes
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if (destination.y > position.y) position += Vector2Int.up;
            else if (destination.y < position.y) position += Vector2Int.down;
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x) position += Vector2Int.right;
            else if (destination.x < position.x) position += Vector2Int.left;
            corridor.Add(position);
        }
        return IncreaseCorridorBrush3by3(corridor);
    }

<<<<<<< Updated upstream
    // DODANA METODA POGRUBIAJï¿½CA (Dostosowana do HashSet)
=======
>>>>>>> Stashed changes
    private HashSet<Vector2Int> IncreaseCorridorBrush3by3(HashSet<Vector2Int> corridor)
    {
        HashSet<Vector2Int> newCorridor = new HashSet<Vector2Int>();
        foreach (var pos in corridor)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++) newCorridor.Add(pos + new Vector2Int(x, y));
            }
        }
        return newCorridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }
}