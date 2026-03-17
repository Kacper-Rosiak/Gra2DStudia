using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyFactory factory;
    public GameObject enemyPrefab;

    void Start()
    {
        Enemy enemy = factory.CreateEnemy("Goblin");

        GameObject obj = Instantiate(enemyPrefab);
        obj.GetComponent<EnemyView>().Init(enemy);
    }
}