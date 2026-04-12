using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerPageUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private Image classIconImage;

    [Header("Data Provider")]
    [SerializeField] private PlayerManager playerManager;

    private void OnEnable()
    {
        if (playerManager == null)
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
        }

        if (playerManager != null && playerManager.Stats != null)
        {
            playerManager.Stats.OnStatsChanged += UpdateUI;
            playerManager.Stats.OnHealthChanged += HandleHealthChanged;
            playerManager.Stats.OnLevelUp += HandleLevelUp;
            UpdateUI();
        }
    }

    private void OnDisable()
    {
        if (playerManager != null && playerManager.Stats != null)
        {
            playerManager.Stats.OnStatsChanged -= UpdateUI;
            playerManager.Stats.OnHealthChanged -= HandleHealthChanged;
            playerManager.Stats.OnLevelUp -= HandleLevelUp;
        }
    }

    private void HandleHealthChanged(int current, int max) => UpdateUI();
    private void HandleLevelUp(int level) => UpdateUI();

    public void UpdateUI()
    {
        if (playerManager == null)
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
        }

        if (playerManager != null && playerManager.Stats != null)
        {
            // Podstawowe statystyki z zabezpieczeniem przed null
            if (playerNameText != null) playerNameText.text = $"Name: {playerManager.playerName}";
            if (levelText != null) levelText.text = $"Level: {playerManager.Stats.Level}";
            if (hpText != null) hpText.text = $"HP: {playerManager.Stats.CurrentHP} / {playerManager.Stats.MaxHP}";
            if (attackText != null) attackText.text = $"Attack: {playerManager.Stats.Attack}";
            if (defenseText != null) defenseText.text = $"Defense: {playerManager.Stats.Defense}";

            // Obrazek klasy
            if (classIconImage != null)
            {
                if (playerManager.startingClass != null && playerManager.startingClass.classIcon != null)
                {
                    classIconImage.sprite = playerManager.startingClass.classIcon;
                    classIconImage.enabled = true;
                }
                else
                {
                    classIconImage.enabled = false;
                }
            }
        }
        else
        {
            Debug.LogWarning("PlayerPageUI: Brak referencji do PlayerManager lub statystyk!");
        }
    }
}
