using UnityEngine;
using System;

[Serializable]
public class LootItem
{
    public string itemId;
    public float dropChance;
}

[CreateAssetMenu(fileName = "LootTable", menuName = "Game/LootTable")]
public class LootTable : ScriptableObject
{
    public LootItem[] items;

    public string RollLoot()
    {
        foreach (var item in items)
        {
            if (UnityEngine.Random.value <= item.dropChance)
                return item.itemId;
        }

        return null;
    }
}