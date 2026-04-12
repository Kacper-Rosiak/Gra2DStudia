using UnityEngine;

public class Slot : MonoBehaviour
{
    public ItemType? allowedType; // Null = any type (backpack), otherwise specific slot
    public ItemData currentItemData;
    public GameObject currentItemVisual; // The UI element with the icon

    public bool IsFull => currentItemData != null;

    public void SetItem(ItemData item, GameObject visualPrefab)
    {
        ClearSlot();
        currentItemData = item;
        if (item != null && visualPrefab != null)
        {
            currentItemVisual = Instantiate(visualPrefab, transform);
            currentItemVisual.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    public void ClearSlot()
    {
        currentItemData = null;
        if (currentItemVisual != null)
        {
            Destroy(currentItemVisual);
        }
    }
}
