using UnityEngine;

public class EnemyView : MonoBehaviour
{
    private Enemy enemy;

    public void Init(Enemy model)
    {
        enemy = model;

        enemy.OnEnemyDeath += HandleDeath;
    }

    public void TakeDamage(int dmg)
    {
        enemy.TakeDamage(dmg);
    }

    private void HandleDeath(int xp)
    {
        Debug.Log("Enemy died, XP: " + xp);

        Destroy(gameObject);
    }
}