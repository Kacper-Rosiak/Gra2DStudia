using UnityEngine;

public class AchievementTester : MonoBehaviour
{
    // To pole pokaże się w Inspektorze - tu wrzucisz swój Canvas
    public AchievementUIController ui;

    // To pole pokaże się w Inspektorze - tu wrzucisz obrazek gold
    public Sprite grafikaZlota;

    void Update()
    {
        // Jeśli naciśniesz klawisz T...
        if (Input.GetKeyDown(KeyCode.T))
        {
            // ...wyślij sygnał do Twojego głównego skryptu, żeby pokazał obrazek
            if (ui != null) ui.ShowAchievement(grafikaZlota);
        }
    }
}