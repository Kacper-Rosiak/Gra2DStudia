using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Przyciski")]
    public Button btnContinue;
    public Button btnNewGame;
    public Button btnLoadGame;
    public Button btnQuit;

    [Header("Panele")]
    public GameObject loadGamePanel;

    private SaveManager saveManager;

    void Start()
    {
        // Tworzymy instancjê naszej logiki z pierwszego skryptu
        saveManager = new SaveManager();

        // Podpinamy funkcje pod klikniêcia
        btnContinue.onClick.AddListener(OnContinueClick);
        btnNewGame.onClick.AddListener(OnNewGameClick);
        btnLoadGame.onClick.AddListener(OnLoadGameClick);
        btnQuit.onClick.AddListener(OnQuitClick);

        // Zabezpieczenie z treœci zadania: Wygaszamy przycisk, jeœli brak zapisów
        if (!saveManager.HasAnySave())
        {
            btnContinue.interactable = false;
            // Opcjonalnie blokujemy te¿ menu wczytywania, bo i tak jest puste
            btnLoadGame.interactable = false;
        }
    }

    void OnContinueClick()
    {
        saveManager.LoadLatestSave();
        // £aduje scenê "Obozowisko" asynchronicznie (ma indeks 1 w Build Profiles)
        SceneManager.LoadSceneAsync(1);
    }

    void OnNewGameClick()
    {
        saveManager.InitializeNewGame();
        SceneManager.LoadSceneAsync(1);
    }

    void OnLoadGameClick()
    {
        // W³¹cza ten ukryty panel
        loadGamePanel.SetActive(true);
    }

    void OnQuitClick()
    {
        Debug.Log("Wychodzenie z aplikacji...");
        Application.Quit();
    }
}