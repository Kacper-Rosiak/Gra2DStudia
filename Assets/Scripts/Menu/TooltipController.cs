using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class TooltipController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemStatsText;
    public TextMeshProUGUI itemDescriptionText;

    private RectTransform _rectTransform;
    private Canvas _parentCanvas;

    private void Awake()
    {
        if (tooltipPanel != null)
        {
            _rectTransform = tooltipPanel.GetComponent<RectTransform>();
            HideTooltip();
        }
        else
        {
            Debug.LogError("[TooltipController] 'tooltipPanel' is not assigned in the Inspector!", this);
        }
        _parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        if (_rectTransform == null) return;

        Vector3 mousePos = Input.mousePosition;
        
        float tooltipWidth = _rectTransform.rect.width;
        float tooltipHeight = _rectTransform.rect.height;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector3 offset = new Vector3(25, -25, 0);

        if (mousePos.x + offset.x + tooltipWidth > screenWidth)
        {
            offset.x = -tooltipWidth - 10;
        }

        if (mousePos.y + offset.y - tooltipHeight < 0)
        {
            offset.y = tooltipHeight + 10;
        }

        _rectTransform.position = mousePos + offset;
    }

    public void ShowTooltip(ItemData item)
    {
        if (item == null) return;
        if (tooltipPanel == null)
        {
            Debug.LogWarning("[TooltipController] Cannot show tooltip: 'tooltipPanel' is missing.");
            return;
        }

        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = item.description;

        StringBuilder sb = new StringBuilder();
        if (item.type == ItemType.Potion)
        {
            if (item.healAmount > 0) sb.AppendLine($"<color=#00FF00>Leczenie: +{item.healAmount} HP</color>");
        }
        else
        {
            if (item.bonusAttack > 0) sb.AppendLine($"Atak: +{item.bonusAttack}");
            if (item.bonusDefense > 0) sb.AppendLine($"Obrona: +{item.bonusDefense}");
            if (item.bonusMaxHP > 0) sb.AppendLine($"Maks. HP: +{item.bonusMaxHP}");
        }

        if (itemStatsText != null) itemStatsText.text = sb.ToString();
        
        tooltipPanel.SetActive(true);

        // Wymuszenie natychmiastowego przeliczenia rozmiaru tła
        if (_rectTransform != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}
