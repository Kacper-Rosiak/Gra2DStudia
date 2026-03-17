// Enemy.cs
using System;

public class Enemy : Entity
{
    public string EnemyId { get; private set; }
    public int XPReward { get; private set; }

    public event Action<int> OnEnemyDeath;

    public Enemy(EnemyData data)
    {
        IsPlayer = false; // Definiujemy, ¿e to przeciwnik
        Name = data.enemyId; // Mo¿esz tu przypisaæ inn¹ nazwê z danych, jeœli masz
        EnemyId = data.enemyId;

        MaxHP = data.maxHP;
        CurrentHP = MaxHP;
        Attack = data.attack;
        Defense = data.defense;
        Speed = data.speed;
        XPReward = data.xpReward;

        // DodgeChance pozostaje domyœlnie na 0, bo nie zmieniamy go w konstruktorze
    }

    public override void TakeDamage(int damage)
    {
        // Obra¿enia zosta³y ju¿ pomniejszone o pancerz w AttackCommand, 
        // wiêc tu tylko odejmujemy punkty zdrowia.
        base.TakeDamage(damage);

        if (!IsAlive())
        {
            OnEnemyDeath?.Invoke(XPReward);
        }
    }
}