// Entity.cs
using System;
public abstract class Entity : ICombatEntity
{
    public string Name { get; set; }
    public bool IsPlayer { get; set; }

    // Dodano "virtual", aby klasy dziedzicz¹ce (Player) mog³y to nadpisaæ
    public virtual int MaxHP { get; protected set; }
    public virtual int CurrentHP { get; protected set; }
    public virtual int Attack { get; protected set; }
    public virtual int Defense { get; protected set; }
    public virtual int Speed { get; protected set; }
    public virtual int DodgeChance { get; protected set; } = 0;
    public bool IsStunned { get; set; } = false;
    public Action<Entity, Action<string>> OnTurnStart;
    public virtual void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP < 0)
        {
            CurrentHP = 0;
        }
    }

    public bool IsAlive()
    {
        return CurrentHP > 0;
    }
    public void TriggerTurnStartEffects(Action<string> logCallback)
    {
        OnTurnStart?.Invoke(this, logCallback);
    }
}