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
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerManager == null)
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
        }

        if (playerManager != null && playerManager.Stats != null)
        {
            // Podstawowe statystyki
            playerNameText.text = $"Name: {playerManager.playerName}";
            levelText.text = $"Level: {playerManager.Stats.Level}";
            hpText.text = $"HP: {playerManager.Stats.CurrentHP} / {playerManager.Stats.MaxHP}";
            attackText.text = $"Attack: {playerManager.Stats.Attack}";
            defenseText.text = $"Defense: {playerManager.Stats.Defense}";

            // Obrazek klasy
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
        else
        {
            Debug.LogWarning("PlayerPageUI: Brak referencji do PlayerManager lub statystyk!");
        }
    }
}
