using UnityEngine;

public class MenuController : MonoBehaviour
{
    private static MenuController _instance;

    [Header("Menus")]
    public GameObject inventoryCanvas; // Okno pod klawisz 'I'
    public GameObject systemMenuCanvas; // Okno pod klawisz 'ESC'

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Upewnij się, że na start wszystko jest zamknięte
        if (inventoryCanvas != null) inventoryCanvas.SetActive(false);
        if (systemMenuCanvas != null) systemMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Obsługa klawisza I (Ekwipunek / Postać)
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleMenu(inventoryCanvas, systemMenuCanvas);
        }

        // Obsługa klawisza ESC (Ustawienia / Osiągnięcia)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu(systemMenuCanvas, inventoryCanvas);
        }
    }

    private void ToggleMenu(GameObject target, GameObject other)
    {
        if (target == null) return;

        bool isOpening = !target.activeSelf;
        
        // Jeśli otwieramy to okno, upewnij się że drugie jest zamknięte
        if (isOpening && other != null)
        {
            other.SetActive(false);
        }

        target.SetActive(isOpening);

        // Zarządzanie pauzą
        UpdatePauseState();
    }

    private void UpdatePauseState()
    {
        bool isAnyMenuOpen = (inventoryCanvas != null && inventoryCanvas.activeSelf) || 
                             (systemMenuCanvas != null && systemMenuCanvas.activeSelf);
        
        Time.timeScale = isAnyMenuOpen ? 0f : 1f;
        
        // Opcjonalnie: odblokuj kursor myszy jeśli menu jest otwarte
        Cursor.visible = isAnyMenuOpen;
        Cursor.lockState = isAnyMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
