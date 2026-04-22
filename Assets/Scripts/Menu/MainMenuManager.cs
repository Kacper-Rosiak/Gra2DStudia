using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartNewGame()
    {
        // Tutaj wpisz DOKŁADNĄ nazwę sceny, na której masz ten piękny loch z wyborem postaci
        SceneManager.LoadScene("WyborPostaci");
    }
}