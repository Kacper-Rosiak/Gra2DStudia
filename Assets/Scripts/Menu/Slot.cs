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
            RectTransform rt = currentItemVisual.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }
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
