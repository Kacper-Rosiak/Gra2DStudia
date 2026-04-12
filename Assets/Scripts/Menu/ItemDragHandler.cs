using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform _originalParent;
    private CanvasGroup _canvasGroup;
    private InventoryController _controller;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _controller = FindFirstObjectByType<InventoryController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent = transform.parent;
        
        // Przenieś na wierzch hierarchii podczas przeciągania
        transform.SetParent(transform.root);
        
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.alpha = 1f;

        // Znajdź slot pod myszką
        Slot dropSlot = null;
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (var result in raycastResults)
        {
            dropSlot = result.gameObject.GetComponent<Slot>();
            if (dropSlot != null) break;
        }

        if (dropSlot != null)
        {
            ItemView itemView = GetComponent<ItemView>();
            ItemData draggedItem = itemView.GetData();
            Slot originalSlot = _originalParent.GetComponent<Slot>();

            // Logika zakładania sprzętu (to co było wcześniej)
            if (dropSlot.allowedType != null)
            {
                if (draggedItem.type == dropSlot.allowedType)
                {
                    EquipViaDrag(draggedItem);
                }
                else
                {
                    ReturnToOriginal();
                }
            }
            else if (originalSlot.allowedType != null && dropSlot.allowedType == null)
            {
                UnequipViaDrag(draggedItem);
            }
            else
            {
                ReturnToOriginal();
            }
        }
        else
        {
            ReturnToOriginal();
        }

        // WAŻNE: Włączamy blokowanie raycastów dopiero NA SAMYM KOŃCU
        _canvasGroup.blocksRaycasts = true;
    }

    private void EquipViaDrag(ItemData item)
    {
        PlayerManager player = FindFirstObjectByType<PlayerManager>();
        if (player != null)
        {
            ItemData oldItem = player.Equipment.EquipItem(item);
            player.Inventory.RemoveItem(item);
            if (oldItem != null)
            {
                player.Inventory.AddItem(oldItem);
            }
            // InventoryController automatycznie odświeży UI dzięki eventom
        }
    }

    private void UnequipViaDrag(ItemData item)
    {
        PlayerManager player = FindFirstObjectByType<PlayerManager>();
        if (player != null)
        {
            ItemData unequippedItem = player.Equipment.UnequipItem(item.type);
            if (unequippedItem != null)
            {
                player.Inventory.AddItem(unequippedItem);
            }
        }
    }

    private void ReturnToOriginal()
    {
        transform.SetParent(_originalParent);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
