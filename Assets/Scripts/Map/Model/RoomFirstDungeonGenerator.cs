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
    [SerializeField] private GameObject startingRoomPrefab;
    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private List<GameObject> enemyPrefabsAscending;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private Transform entityParent;
    [SerializeField] private int maxEnemiesPerDungeon = 10;
    [SerializeField] private int maxChestsPerDungeon = 2;
    [SerializeField] private int minEnemyDistanceFromWall = 2;
    [SerializeField] private bool preventSpawnInPlayerRoom = true;

    [Header("PCG - Environment Decorations")]
    [SerializeField] private GameObject torchPrefab;
    [SerializeField][Range(0f, 1f)] private float torchSpawnChance = 0.3f;
    [SerializeField] private float minDistanceBetweenTorches = 4f;

    [Header("PCG - Floor Props")]
    [SerializeField] private List<GameObject> floorDecorationPrefabs;
    [SerializeField] private int minDecorationsPerRoom = 3;
    [SerializeField] private int maxDecorationsPerRoom = 6;
    [SerializeField] private int maxDecorationsDistanceFromWall = 2;
    [SerializeField] private float minDistanceBetweenDecorations = 5f;

    [Header("PCG - Traps")]
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private int maxTrapsPerRoom = 2;
    [SerializeField][Range(0f, 1f)] private float trapSpawnChance = 0.5f;
    [SerializeField] private float minDistanceBetweenTraps = 3.5f;

    // --- NOWA, PEŁNA KONTROLA NAD MIOTACZAMI W INSPEKTORZE ---
    [Header("PCG - Wall Flamethrowers")]
    [SerializeField] private GameObject topFlamethrowerPrefab;
    [SerializeField] private GameObject sideFlamethrowerPrefab;
    [SerializeField][Range(0f, 1f)] private float flamethrowerSpawnChance = 0.15f;
    [SerializeField] private float minDistanceBetweenFlamethrowers = 5f;
    [SerializeField] private bool spawnFlamethrowersOnlyInCorridors = true;

    [Tooltip("Przesunięcie w pionie dla górnego miotacza")]
    [SerializeField][Range(-1f, 1f)] private float topFlamethrowerYOffset = -0.2f;

    [Tooltip("Przesunięcie w poziomie dla LEWEJ ściany (wypchnięcie w stronę korytarza)")]
    [SerializeField][Range(-1f, 1f)] private float leftWallXOffset = 0.4f;
    [Tooltip("Zaznacz, jeśli miotacz na LEWEJ ścianie patrzy w złą stronę")]
    [SerializeField] private bool flipLeftWall = false;

    [Tooltip("Przesunięcie w poziomie dla PRAWEJ ściany (wypchnięcie w stronę korytarza)")]
    [SerializeField][Range(-1f, 1f)] private float rightWallXOffset = -0.4f;
    [Tooltip("Zaznacz, jeśli miotacz na PRAWEJ ścianie patrzy w złą stronę")]
    [SerializeField] private bool flipRightWall = true;

    private HashSet<Vector2Int> _occupiedWallPositions = new HashSet<Vector2Int>();

    protected override void RunProceduralGeneration()
    {
        if (entityParent != null)
        {
            for (int i = entityParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(entityParent.GetChild(i).gameObject);
            }
        }
        _occupiedWallPositions.Clear();
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

        if (roomCenters.Count > 0)
        {
            Vector3 startRoomCenterPos = new Vector3(roomCenters[0].x, roomCenters[0].y, 0);

            if (startingRoomPrefab != null)
            {
                Vector3 prefabPos = startRoomCenterPos + new Vector3(0f, 1.5f, 0f);
                Instantiate(startingRoomPrefab, prefabPos, Quaternion.identity, entityParent);
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null && PlayerSpawner.Instance != null)
            {
                player = PlayerSpawner.Instance.SpawnPlayer();
            }

            if (player != null)
            {
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.position = startRoomCenterPos;
                player.transform.position = startRoomCenterPos;
                if (PlayerSpawner.Instance != null) PlayerSpawner.Instance.AssignCamera(player);
            }
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);
        tilemapVisualizer.CompressAllBounds();

        PopulateRooms(roomsList, floor, corridors);
        SpawnWallFlamethrowers(floor, corridors);
        SpawnTorches(floor);
    }

    private void SpawnWallFlamethrowers(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> corridorsPositions)
    {
        if (topFlamethrowerPrefab == null && sideFlamethrowerPrefab == null) return;

        List<Vector2Int> spawnedFlamethrowers = new List<Vector2Int>();
        IEnumerable<Vector2Int> positionsToEvaluate = spawnFlamethrowersOnlyInCorridors ? corridorsPositions : floorPositions;

        foreach (Vector2Int pos in positionsToEvaluate)
        {
            if (Random.value > flamethrowerSpawnChance) continue;

            // 1. ŚCIANA GÓRNA (Strzał w dół)
            if (topFlamethrowerPrefab != null && !floorPositions.Contains(pos + Vector2Int.up))
            {
                Vector2Int targetWallPos = pos + Vector2Int.up;
                if (!_occupiedWallPositions.Contains(targetWallPos) && !IsTooCloseToOthers(targetWallPos, spawnedFlamethrowers))
                {
                    Vector3 spawnPos = new Vector3(targetWallPos.x + 0.5f, targetWallPos.y + 0.5f + topFlamethrowerYOffset, 0f);
                    Instantiate(topFlamethrowerPrefab, spawnPos, Quaternion.identity, entityParent);

                    spawnedFlamethrowers.Add(targetWallPos);
                    _occupiedWallPositions.Add(targetWallPos);
                    continue;
                }
            }

            // 2. ŚCIANA LEWA (Miotacz po lewej stronie korytarza, strzał w prawo)
            if (sideFlamethrowerPrefab != null && !floorPositions.Contains(pos + Vector2Int.left))
            {
                Vector2Int targetWallPos = pos + Vector2Int.left;
                if (!_occupiedWallPositions.Contains(targetWallPos) && !IsTooCloseToOthers(targetWallPos, spawnedFlamethrowers))
                {
                    Vector3 spawnPos = new Vector3(targetWallPos.x + 0.5f + leftWallXOffset, targetWallPos.y + 0.5f, 0f);
                    GameObject ftObj = Instantiate(sideFlamethrowerPrefab, spawnPos, Quaternion.identity, entityParent);

                    ftObj.transform.localScale = new Vector3(flipLeftWall ? -1f : 1f, 1f, 1f);

                    spawnedFlamethrowers.Add(targetWallPos);
                    _occupiedWallPositions.Add(targetWallPos);
                    continue;
                }
            }

            // 3. ŚCIANA PRAWA (Miotacz po prawej stronie korytarza, strzał w lewo)
            if (sideFlamethrowerPrefab != null && !floorPositions.Contains(pos + Vector2Int.right))
            {
                Vector2Int targetWallPos = pos + Vector2Int.right;
                if (!_occupiedWallPositions.Contains(targetWallPos) && !IsTooCloseToOthers(targetWallPos, spawnedFlamethrowers))
                {
                    Vector3 spawnPos = new Vector3(targetWallPos.x + 0.5f + rightWallXOffset, targetWallPos.y + 0.5f, 0f);
                    GameObject ftObj = Instantiate(sideFlamethrowerPrefab, spawnPos, Quaternion.identity, entityParent);

                    ftObj.transform.localScale = new Vector3(flipRightWall ? -1f : 1f, 1f, 1f);

                    spawnedFlamethrowers.Add(targetWallPos);
                    _occupiedWallPositions.Add(targetWallPos);
                    continue;
                }
            }
        }
    }

    private bool IsTooCloseToOthers(Vector2Int targetPos, List<Vector2Int> spawnedList)
    {
        foreach (Vector2Int sPos in spawnedList)
        {
            if (Vector2.Distance(targetPos, sPos) < minDistanceBetweenFlamethrowers) return true;
        }
        return false;
    }

    private void SpawnTorches(HashSet<Vector2Int> floorPositions)
    {
        if (torchPrefab == null) return;

        List<Vector2Int> topWalls = new List<Vector2Int>();
        foreach (var pos in floorPositions)
        {
            Vector2Int upPos = pos + Vector2Int.up;
            if (!floorPositions.Contains(upPos) && !_occupiedWallPositions.Contains(upPos))
            {
                topWalls.Add(upPos);
            }
        }

        List<Vector2Int> spawnedTorches = new List<Vector2Int>();
        foreach (var wallPos in topWalls)
        {
            if (Random.value <= torchSpawnChance)
            {
                bool isTooClose = false;
                foreach (var spawnedPos in spawnedTorches)
                {
                    if (Vector2.Distance(wallPos, spawnedPos) < minDistanceBetweenTorches)
                    {
                        isTooClose = true;
                        break;
                    }
                }

                if (!isTooClose)
                {
                    Vector3 spawnPos = new Vector3(wallPos.x + 0.5f, wallPos.y + 0.5f, 0f);
                    Instantiate(torchPrefab, spawnPos, Quaternion.identity, entityParent);
                    spawnedTorches.Add(wallPos);
                    _occupiedWallPositions.Add(wallPos);
                }
            }
        }
    }

    private void PopulateRooms(List<BoundsInt> rooms, HashSet<Vector2Int> floor, HashSet<Vector2Int> corridors)
    {
        Vector2Int startPos = (Vector2Int)startPosition;
        float maxDistance = 0f;
        BoundsInt furthestRoom = rooms[0];
        int totalEnemiesSpawned = 0;
        int totalChestsSpawned = 0;

        foreach (var room in rooms)
        {
            float dist = Vector2.Distance(startPos, (Vector2Int)Vector3Int.RoundToInt(room.center));
            if (dist > maxDistance)
            {
                maxDistance = dist;
                furthestRoom = room;
            }
        }

        bool isFirstRoom = true;
        foreach (var room in rooms)
        {
            if (preventSpawnInPlayerRoom && isFirstRoom)
            {
                isFirstRoom = false;
                continue;
            }
            isFirstRoom = false;

            Vector2Int idealCenter = (Vector2Int)Vector3Int.RoundToInt(room.center);
            float distanceFromStart = Vector2.Distance(startPos, idealCenter);

            if (distanceFromStart < 2f) continue;

            float difficultyFactor = maxDistance > 0 ? (distanceFromStart / maxDistance) : 0f;

            List<Vector2Int> safePoints = GetSafeSpawnPoints(room, floor, corridors);
            if (safePoints.Count == 0) continue;

            if (room.Equals(furthestRoom) && exitPrefab != null)
            {
                Vector2Int exitPos = FindBestPositionNearCenter(idealCenter, safePoints);
                Instantiate(exitPrefab, new Vector3(exitPos.x + 0.5f, exitPos.y + 0.5f, 0), Quaternion.identity, entityParent);
                safePoints.Remove(exitPos);
            }

            if (totalChestsSpawned < maxChestsPerDungeon && safePoints.Count > 0)
            {
                if (Random.value < 0.6f)
                {
                    Vector2Int bestChestPos = FindBestPositionNearCenter(idealCenter, safePoints);
                    SpawnChest(bestChestPos, difficultyFactor);
                    totalChestsSpawned++;
                    safePoints.Remove(bestChestPos);
                }
            }

            if (totalEnemiesSpawned < maxEnemiesPerDungeon && safePoints.Count > 0)
            {
                List<Vector2Int> enemySafePoints = GetPointsAwayFromWall(safePoints, floor, minEnemyDistanceFromWall);
                if (enemySafePoints.Count > 0)
                {
                    Vector2Int pos = GetAndRemoveRandomPoint(enemySafePoints);
                    SpawnEnemy(pos, difficultyFactor);
                    totalEnemiesSpawned++;
                    safePoints.Remove(pos);
                }
            }

            if (floorDecorationPrefabs != null && floorDecorationPrefabs.Count > 0 && safePoints.Count > 0)
            {
                List<Vector2Int> decorSafePoints = GetPointsNearWall(safePoints, floor, maxDecorationsDistanceFromWall);
                if (decorSafePoints.Count == 0) decorSafePoints = new List<Vector2Int>(safePoints);

                int minCount = Mathf.Max(3, minDecorationsPerRoom);
                int maxCount = Mathf.Max(minCount, maxDecorationsPerRoom);
                int decorationsToSpawn = Random.Range(minCount, maxCount + 1);

                HashSet<GameObject> usedPrefabsInThisRoom = new HashSet<GameObject>();

                for (int i = 0; i < decorationsToSpawn; i++)
                {
                    if (decorSafePoints.Count == 0) break;

                    List<GameObject> availablePrefabs = floorDecorationPrefabs.FindAll(p => !usedPrefabsInThisRoom.Contains(p));
                    if (availablePrefabs.Count == 0) break;

                    int randomDecorIndex = Random.Range(0, availablePrefabs.Count);
                    GameObject decorPrefab = availablePrefabs[randomDecorIndex];

                    if (decorPrefab != null)
                    {
                        Vector2Int decorPos = GetAndRemoveRandomPoint(decorSafePoints);
                        Vector3 spawnPos = new Vector3(decorPos.x + 0.5f, decorPos.y + 0.5f, 0f);

                        Instantiate(decorPrefab, spawnPos, Quaternion.identity, entityParent);
                        usedPrefabsInThisRoom.Add(decorPrefab);

                        decorSafePoints.RemoveAll(p => Vector2.Distance(decorPos, p) < minDistanceBetweenDecorations);
                        safePoints.Remove(decorPos);
                    }
                }
            }

            if (trapPrefab != null && safePoints.Count > 0 && Random.value < trapSpawnChance)
            {
                int trapsToSpawn = Random.Range(1, maxTrapsPerRoom + 1);
                for (int i = 0; i < trapsToSpawn; i++)
                {
                    if (safePoints.Count == 0) break;

                    Vector2Int trapPos = GetAndRemoveRandomPoint(safePoints);
                    Vector3 spawnPos = new Vector3(trapPos.x + 0.5f, trapPos.y + 0.5f, 0f);

                    Instantiate(trapPrefab, spawnPos, Quaternion.identity, entityParent);
                    safePoints.RemoveAll(p => Vector2.Distance(trapPos, p) < minDistanceBetweenTraps);
                }
            }
        }
    }

    private List<Vector2Int> GetPointsNearWall(List<Vector2Int> safePoints, HashSet<Vector2Int> floor, int maxDistance)
    {
        List<Vector2Int> filteredPoints = new List<Vector2Int>();
        foreach (Vector2Int pos in safePoints)
        {
            bool isNearWall = false;
            for (int x = -maxDistance; x <= maxDistance; x++)
            {
                for (int y = -maxDistance; y <= maxDistance; y++)
                {
                    if (!floor.Contains(pos + new Vector2Int(x, y)))
                    {
                        isNearWall = true;
                        break;
                    }
                }
                if (isNearWall) break;
            }
            if (isNearWall) filteredPoints.Add(pos);
        }
        return filteredPoints;
    }

    private List<Vector2Int> GetPointsAwayFromWall(List<Vector2Int> safePoints, HashSet<Vector2Int> floor, int requiredDistance)
    {
        List<Vector2Int> filteredPoints = new List<Vector2Int>();
        foreach (Vector2Int pos in safePoints)
        {
            bool isFarEnough = true;
            for (int x = -requiredDistance; x <= requiredDistance; x++)
            {
                for (int y = -requiredDistance; y <= requiredDistance; y++)
                {
                    if (!floor.Contains(pos + new Vector2Int(x, y)))
                    {
                        isFarEnough = false;
                        break;
                    }
                }
                if (!isFarEnough) break;
            }
            if (isFarEnough) filteredPoints.Add(pos);
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