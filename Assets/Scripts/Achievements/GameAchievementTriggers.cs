using UnityEngine;

public class GameAchievementTriggers : MonoBehaviour
{
    public AchievementUIController ui;

    [Header("Grafiki Osiągnięć")]
    public Sprite grafikaKill;
    public Sprite grafikaLevelUp;
    public Sprite grafikaSpecjal; // Tutaj wrzuć "Kolekcjonera"

    // Statyki, aby osiągnięcia odblokowały się tylko raz na całą sesję gry
    private static bool unlockedKill = false;
    private static bool unlockedLevelUp = false;
    private static bool unlockedSpecial = false;

    private int specialUseCount = 0;

    public void TriggerKillAchievement()
    {
        if (!unlockedKill && grafikaKill != null)
        {
            unlockedKill = true;
            ui.ShowAchievement(grafikaKill);
        }
    }

    public void TriggerLevelUp()
    {
        // Level up pozwalamy pokazywać wielokrotnie przy każdym awansie
        if (grafikaLevelUp != null)
        {
            ui.ShowAchievement(grafikaLevelUp);
        }
    }

    public void TriggerSpecialAchievement()
    {
        if (unlockedSpecial) return;

        specialUseCount++;
        Debug.Log($"Licznik specjalny: {specialUseCount}/3");

        if (specialUseCount >= 3 && grafikaSpecjal != null)
        {
            unlockedSpecial = true;
            ui.ShowAchievement(grafikaSpecjal);
        }
    }
}