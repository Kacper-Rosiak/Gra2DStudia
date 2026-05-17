using UnityEngine;
using UnityEngine.UI;

public class AchievementSlotUI : MonoBehaviour
{
    [Header("Achievement Data")]
    public string achievementId; // Musi pasować do ID w AchievementDefinition

    [Header("UI Reference")]
    public Image achievementImage; // Główny obrazek zawierający wszystko

    [Header("Settings")]
    public Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color unlockedColor = Color.white;

    public void SetState(bool isUnlocked)
    {
        if (achievementImage != null)
        {
            achievementImage.color = isUnlocked ? unlockedColor : lockedColor;
        }
    }
}
