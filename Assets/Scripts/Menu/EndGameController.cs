using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Kontroler sceny EndGame. Obsługuje przyciski powrotu do menu oraz ponownej gry.
/// </summary>
public class EndGameController : MonoBehaviour
{
    /// <summary>
    /// Metoda wywoływana po kliknięciu "Zagraj ponownie".
    /// Przenosi do wyboru postaci, by stworzyć nową postać.
    /// </summary>
    public void ZagrajPonownie()
    {
        CleanupPlayer();
        SceneManager.LoadScene("CharacterSelection");
    }

    /// <summary>
    /// Metoda wywoływana po kliknięciu "Menu główne".
    /// </summary>
    public void WrocDoMenu()
    {
        CleanupPlayer();
        SceneManager.LoadScene("MenuGlowne");
    }

    /// <summary>
    /// Niszczy obiekt PlayerManager, aby nie przeszedł do nowej gry jako "martwy" singleton.
    /// </summary>
    private void CleanupPlayer()
    {
        if (PlayerManager.Instance != null)
        {
            Destroy(PlayerManager.Instance.gameObject);
        }

        if (CombatTransitionManager.Instance != null)
        {
            CombatTransitionManager.Instance.ResetState();
        }

        if (QuestManager.Instance != null)
        {
            Destroy(QuestManager.Instance.gameObject);
        }
        
        // Opcjonalnie: upewniamy się, że SaveManager nie ma załadowanych danych, 
        // które mogłyby wpłynąć na nową grę.
        SaveManager.CurrentSaveData = null;
    }
}
