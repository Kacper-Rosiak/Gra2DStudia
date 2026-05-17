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
        // Tworzymy instancję naszej logiki
        saveManager = new SaveManager();

        // Podpinamy funkcje pod kliknięcia
        btnContinue.onClick.AddListener(OnContinueClick);
        btnNewGame.onClick.AddListener(OnNewGameClick);
        btnLoadGame.onClick.AddListener(OnLoadGameClick);
        btnQuit.onClick.AddListener(OnQuitClick);

        // Przycisk kontynuacji jest dostępny tylko gdy istnieje jakikolwiek zapis
        btnContinue.interactable = saveManager.HasAnySave();

        // Przycisk wczytywania (widoku 3 slotów) jest zawsze dostępny zgodnie z planem
        btnLoadGame.interactable = true; 
    }

    void OnContinueClick()
    {
        saveManager.LoadLatestSave();

        if (SaveManager.CurrentSaveData != null)
        {
            string sceneToLoad = SaveManager.CurrentSaveData.sceneName;
            if (string.IsNullOrEmpty(sceneToLoad)) sceneToLoad = "CampScene"; // Fallback

            SceneManager.LoadSceneAsync(sceneToLoad);
        }
    }


    void OnNewGameClick()
    {
        saveManager.InitializeNewGame();
        SceneManager.LoadSceneAsync(1);
    }

    void OnLoadGameClick()
    {
        // W��cza ten ukryty panel
        loadGamePanel.SetActive(true);
    }

    void OnQuitClick()
    {
        Debug.Log("Wychodzenie z aplikacji...");
        Application.Quit();
    }
}