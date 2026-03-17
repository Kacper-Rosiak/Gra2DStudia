using System;

public class Enemy : Entity
{
    public string EnemyId { get; private set; }

    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int Speed { get; private set; }

    public int XPReward { get; private set; }

    public event Action<int> OnEnemyDeath;

    public Enemy(EnemyData data)
    {
        EnemyId = data.enemyId;

        MaxHP = data.maxHP;
        CurrentHP = MaxHP;

        Attack = data.attack;
        Defense = data.defense;
        Speed = data.speed;

        XPReward = data.xpReward;
    }

    public override void TakeDamage(int damage)
    {
        int finalDamage = Math.Max(0, damage - Defense);

        base.TakeDamage(finalDamage);

        if (!IsAlive())
        {
            OnEnemyDeath?.Invoke(XPReward);
        }
    }
}