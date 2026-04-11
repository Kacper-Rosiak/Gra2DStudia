using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerClass", menuName = "Game/Player Class Data")]
public class PlayerClassData : ScriptableObject
{
    public string className;
    public Sprite classIcon;

    [Header("Base Stats")]
    public int baseMaxHP;
    public int baseAttack;
    public int baseDefense;
    public int baseSpeed;
    public int baseDodgeChance;
}