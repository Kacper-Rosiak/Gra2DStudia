using UnityEngine;
using System.Collections.Generic;
using System.Linq;

# if UNITY_EDITOR
using UnityEditor;
# endif

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "RPG/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();

    public ItemData GetItemByID(string id)
    {
        return allItems.FirstOrDefault(i => i.itemID == id);
    }

#if UNITY_EDITOR
    // Ta metoda wywoła się automatycznie, gdy zmienisz coś w bazie lub gdy projekt się przeładuje
    private void OnValidate()
    {
        RefreshDatabase();
    }

    [ContextMenu("Refresh Database")]
    public void RefreshDatabase()
    {
        // Znajduje wszystkie assety typu ItemData w całym projekcie
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        
        allItems.Clear();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            
            if (item != null && !allItems.Contains(item))
            {
                allItems.Add(item);
            }
        }
        
        // Sortujemy dla porządku
        allItems = allItems.OrderBy(i => i.itemID).ToList();
        
        EditorUtility.SetDirty(this);
        Debug.Log($"ItemDatabase: Automatycznie odświeżono bazę. Znaleziono {allItems.Count} przedmiotów.");
    }
#endif
}
