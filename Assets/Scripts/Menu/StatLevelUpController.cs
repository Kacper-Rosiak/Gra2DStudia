using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class StatLevelUpController : MonoBehaviour
{
    public static StatLevelUpController Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;

    [Header("Stat Texts (Current Values)")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI pointsText;

    [Header("Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defenseButton;
    [SerializeField] private Button hpButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (mainPanel != null) mainPanel.SetActive(false);
        
        attackButton.onClick.AddListener(() => SelectStat("Attack"));
        defenseButton.onClick.AddListener(() => SelectStat("Defense"));
        hpButton.onClick.AddListener(() => SelectStat("HP"));
    }

    private void Start()
    {
        // Sprawdzaj co jakiś czas czy są punkty do rozdania
        InvokeRepeating(nameof(CheckForPendingPoints), 1f, 2f);
    }

    private void CheckForPendingPoints()
    {
        // Nie pokazuj w trakcie walki ani gdy panel jest już otwarty
        if (mainPanel.activeSelf) return;
        
        // Sprawdź czy jesteśmy w CombatScene (proste sprawdzenie po nazwie sceny)
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "CombatScene") return;

        if (PlayerManager.Instance != null && PlayerManager.Instance.Stats != null)
        {
            if (PlayerManager.Instance.Stats.PendingStatPoints > 0)
            {
                ShowPopup();
            }
        }
    }

    public void ShowPopup()
    {
        UpdateUI();
        mainPanel.SetActive(true);
        Time.timeScale = 0f; // Pauza gry
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void UpdateUI()
    {
        var stats = PlayerManager.Instance.Stats;
        if (attackText != null) attackText.text = $"Atak: {stats.Attack}";
        if (defenseText != null) defenseText.text = $"Obrona: {stats.Defense}";
        if (hpText != null) hpText.text = $"Max HP: {stats.MaxHP}";
        if (pointsText != null) pointsText.text = $"Dostępne punkty: {stats.PendingStatPoints}";
    }

    private void SelectStat(string statName)
    {
        var stats = PlayerManager.Instance.Stats;
        if (stats.PendingStatPoints <= 0) return;

        switch (statName)
        {
            case "Attack":
                stats.AddBaseAttack(2);
                break;
            case "Defense":
                stats.AddBaseDefense(2);
                break;
            case "HP":
                stats.AddBaseMaxHP(10);
                break;
        }

        stats.PendingStatPoints--;
        UpdateUI();

        if (stats.PendingStatPoints <= 0)
        {
            ClosePopup();
        }
    }

    private void ClosePopup()
    {
        mainPanel.SetActive(false);
        Time.timeScale = 1f; // Odpauzowanie
    }
}
