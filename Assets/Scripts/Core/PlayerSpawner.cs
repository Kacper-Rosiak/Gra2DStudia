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
    }

    public GameObject SpawnPlayer()
    {
        // Jeśli gracz już jest na scenie, zwróć go
        GameObject existingPlayer = GameObject.FindGameObjectWithTag(playerTag);
        if (existingPlayer != null) return existingPlayer;

        PlayerClassData classToSpawn = GameManager.Instance.SelectedClassData;
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