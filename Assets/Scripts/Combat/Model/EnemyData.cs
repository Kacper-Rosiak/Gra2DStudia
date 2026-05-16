using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyId;

    public int maxHP;
    public int attack;
    public int defense;
    public int speed;

    public bool isBoss;

    public int xpReward;
}