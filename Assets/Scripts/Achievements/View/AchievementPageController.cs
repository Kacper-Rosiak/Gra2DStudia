using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AchievementPageController : MonoBehaviour
{
    [Header("Slots")]
    public List<AchievementSlotUI> achievementSlots = new List<AchievementSlotUI>();

    private void OnEnable()
    {
        RefreshPage();

        // Subskrypcja zdarzenia, aby aktualizować na żywo
        if (AchievementBootstrapper.Instance != null && AchievementBootstrapper.Instance.Achievements != null)
        {
            AchievementBootstrapper.Instance.Achievements.OnAchievementUnlocked += HandleAchievementUnlocked;
        }
    }

    private void OnDisable()
    {
        if (AchievementBootstrapper.Instance != null && AchievementBootstrapper.Instance.Achievements != null)
        {
            AchievementBootstrapper.Instance.Achievements.OnAchievementUnlocked -= HandleAchievementUnlocked;
        }
    }

    public void RefreshPage()
    {
        if (AchievementBootstrapper.Instance == null || AchievementBootstrapper.Instance.Achievements == null)
        {
            Debug.LogWarning("AchievementPageController: Brak instancji AchievementManager!");
            return;
        }

        var progressMap = AchievementBootstrapper.Instance.Achievements.GetProgressForSave();

        foreach (var slot in achievementSlots)
        {
            if (slot == null || string.IsNullOrEmpty(slot.achievementId)) continue;

            bool isUnlocked = false;
            if (progressMap.TryGetValue(slot.achievementId, out var progress))
            {
                isUnlocked = progress.isUnlocked;
            }

            slot.SetState(isUnlocked);
        }
    }

    private void HandleAchievementUnlocked(AchievementDefinition definition)
    {
        // Znajdź slot o pasującym ID i go odśwież
        var slot = achievementSlots.FirstOrDefault(s => s.achievementId == definition.id);
        if (slot != null)
        {
            slot.SetState(true);
        }
    }
}
