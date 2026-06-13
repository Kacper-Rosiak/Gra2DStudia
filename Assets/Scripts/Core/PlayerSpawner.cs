using UnityEngine;
using Unity.Cinemachine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private PlayerClassData fallbackClass;
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Spawnuje gracza na starcie, jeśli jeszcze go nie ma
        SpawnPlayer();

        // POWIADOMIENIE SYSTEMU MISJI (Eksploracja lochów)
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "DungeonScene")
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.ZwiekszPostepCelu("Przeszukaj zapomniane lochy");
            }
        }
    }

    public GameObject SpawnPlayer()
    {
        // 1. Najpierw sprawdźmy, czy mamy już trwałą instancję Singletona (przeszła z innej sceny)
        if (PlayerManager.Instance != null)
        {
            // Przenosimy trwałego gracza na pozycję spawnera w nowej scenie
            PlayerManager.Instance.transform.position = transform.position;
            AssignCamera(PlayerManager.Instance.gameObject);
            return PlayerManager.Instance.gameObject;
        }

        // 2. Jeśli nie ma instancji, szukamy po tagu (wypadek, gdyby Singleton nie był jeszcze gotowy)
        GameObject existingPlayer = GameObject.FindGameObjectWithTag(playerTag);
        if (existingPlayer != null) 
        {
            AssignCamera(existingPlayer);
            return existingPlayer;
        }

        PlayerClassData classToSpawn = null;

        // Jeśli wczytujemy grę, szukamy klasy zapisanej w pliku
        if (SaveManager.CurrentSaveData != null && !string.IsNullOrEmpty(SaveManager.CurrentSaveData.className))
        {
            if (GameManager.Instance.gameDatabase != null)
            {
                classToSpawn = GameManager.Instance.gameDatabase.GetClassByName(SaveManager.CurrentSaveData.className);
            }
        }

        // Jeśli nie wczytujemy zapisu lub nie znaleziono klasy, bierzemy wybraną w menu
        if (classToSpawn == null)
        {
            classToSpawn = GameManager.Instance.SelectedClassData;
        }

        if (classToSpawn == null) classToSpawn = fallbackClass;

        if (classToSpawn != null && classToSpawn.classPrefab != null)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0f);
            GameObject player = Instantiate(classToSpawn.classPrefab, spawnPos, Quaternion.identity);
            player.name = "Player";
            player.tag = playerTag;

            AssignCamera(player);
            return player;
        }

        Debug.LogError("PlayerSpawner: Nie można zespawnować gracza - brak danych!");
        return null;
    }

    public void AssignCamera(GameObject target)
    {
        // 1. Upewnij się, że Main Camera ma CinemachineBrain
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            if (mainCam.GetComponent<CinemachineBrain>() == null)
            {
                mainCam.gameObject.AddComponent<CinemachineBrain>();
                Debug.Log("PlayerSpawner: Dodano brakujący CinemachineBrain do Main Camera.");
            }
        }

        // 2. Szukamy wirtualnej kamery (v3)
        var vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = target.transform;
            vcam.LookAt = target.transform;
            
            // Wymuszamy natychmiastowe przeskoczenie kamery do celu
            vcam.ForceCameraPosition(target.transform.position, Quaternion.identity);
            
            Debug.Log("PlayerSpawner: Kamera podpięta i zresetowana do " + target.name);
        }
        else
        {
            Debug.LogWarning("PlayerSpawner: Nie znaleziono CinemachineCamera na scenie!");
        }
    }
}