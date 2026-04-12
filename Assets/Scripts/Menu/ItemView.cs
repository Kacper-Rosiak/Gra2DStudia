using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    private ItemData _data;
    private InventoryController _controller;
    private TooltipController _tooltip;

    public void Setup(ItemData data, InventoryController controller, TooltipController tooltip)
    {
        _data = data;
        _controller = controller;
        _tooltip = tooltip;
        if (iconImage != null) iconImage.sprite = data.icon;
    }

    public ItemData GetData() => _data;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            if (_tooltip != null) _tooltip.HideTooltip(); // Ukrywamy przy użyciu/założeniu
            _controller.HandleItemDoubleClick(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_tooltip != null && _data != null)
        {
            _tooltip.ShowTooltip(_data);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltip != null)
        {
            _tooltip.HideTooltip();
        }
    }

    // Dodatkowe zabezpieczenie: jeśli przedmiot zostanie usunięty/przeniesiony podczas najechania
    private void OnDisable()
    {
        if (_tooltip != null) _tooltip.HideTooltip();
    }
}
