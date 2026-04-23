using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartNewGame()
    {
        // Nazwa sceny musi pasować do nazwy pliku w Assets/Scenes/
        SceneManager.LoadScene("CharacterSelection");
    }
}