using UnityEngine;
using System.Collections.Generic;

public class EnemyFactory : MonoBehaviour
{
    public List<EnemyData> enemyDatabase;

    private Dictionary<string, EnemyData> enemyMap;

    private void Awake()
    {
        enemyMap = new Dictionary<string, EnemyData>();

        foreach (var data in enemyDatabase)
        {
            enemyMap[data.enemyId] = data;
        }
    }

    public Enemy CreateEnemy(string id)
    {
        if (!enemyMap.ContainsKey(id))
        {
            Debug.LogError("Enemy not found: " + id);
            return null;
        }

        return new Enemy(enemyMap[id]);
    }
}