using UnityEngine;
using System.Collections.Generic;
using System.Linq;

# if UNITY_EDITOR
using UnityEditor;
# endif

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "RPG/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("Items")]
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();

    [Header("Classes")]
    [SerializeField] private List<PlayerClassData> allClasses = new List<PlayerClassData>();

    public ItemData GetItemByID(string id)
    {
        return allItems.FirstOrDefault(i => i.itemID == id);
    }

    public PlayerClassData GetClassByName(string className)
    {
        return allClasses.FirstOrDefault(c => c.className == className);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshDatabase();
    }

    [ContextMenu("Refresh Database")]
    public void RefreshDatabase()
    {
        // 1. Odświeżanie przedmiotów
        string[] itemGuids = AssetDatabase.FindAssets("t:ItemData");
        allItems.Clear();
        foreach (string guid in itemGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item != null) allItems.Add(item);
        }
        allItems = allItems.OrderBy(i => i.itemID).ToList();

        // 2. Odświeżanie klas postaci
        string[] classGuids = AssetDatabase.FindAssets("t:PlayerClassData");
        allClasses.Clear();
        foreach (string guid in classGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PlayerClassData pClass = AssetDatabase.LoadAssetAtPath<PlayerClassData>(path);
            if (pClass != null) allClasses.Add(pClass);
        }
        allClasses = allClasses.OrderBy(c => c.className).ToList();
        
        EditorUtility.SetDirty(this);
        Debug.Log($"ItemDatabase: Odświeżono bazę. Przedmioty: {allItems.Count}, Klasy: {allClasses.Count}");
    }
#endif
}
