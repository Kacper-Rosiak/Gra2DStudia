using UnityEngine;
using UnityEngine.UI;

public class AchievementNotificationUI : MonoBehaviour
{
    [SerializeField] private Text notificationText;
    [SerializeField] private GameObject notificationPanel;

    private void Start()
    {
        notificationPanel.SetActive(false);
        if (AchievementBootstrapper.Instance != null && AchievementBootstrapper.Instance.Achievements != null)
        {
            AchievementBootstrapper.Instance.Achievements.OnAchievementUnlocked += ShowNotification;
        }
    }

    private void OnDestroy()
    {
        if (AchievementBootstrapper.Instance != null && AchievementBootstrapper.Instance.Achievements != null)
        {
            AchievementBootstrapper.Instance.Achievements.OnAchievementUnlocked -= ShowNotification;
        }
    }

    private void ShowNotification(AchievementDefinition data)
    {
        notificationText.text = $"ODBLOKOWANO: {data.displayName}!";
        notificationPanel.SetActive(true);
        CancelInvoke(nameof(HideNotification));
        Invoke(nameof(HideNotification), 3f);
    }

    private void HideNotification()
    {
        notificationPanel.SetActive(false);
    }
}