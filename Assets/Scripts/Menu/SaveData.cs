using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    [Header("Player Identity")]
    public string playerName;
    public string className;

    [Header("Player Stats")]
    public int level;
    public int currentXP;
    public int currentHP;
    public int attack;
    public int defense;
    public int gold;

    [Header("Inventory & Equipment")]
    public List<string> inventoryItemIDs = new List<string>();
    public List<string> equippedItemIDs = new List<string>();

    [Header("World State")]
    public Vector3 playerPosition;
    public string sceneName;

    [Header("Save Metadata")]
    public int saveSlot;
    public string timestamp;
}
