using System;
using UnityEngine;

public abstract class Entity : ICombatEntity
{
    public string Name { get; set; }
    public bool IsPlayer { get; set; }

    // --- TO DODAEM (NADAJNIK) ---
    public event Action<int, int> OnHealthChanged;
    // ----------------------------

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
        if (CurrentHP < 0) CurrentHP = 0;

        // --- TO DODA�EM (SYGNA� DO UI) ---
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        // --------------------------------
    }

    public bool IsAlive() => CurrentHP > 0;

    public void TriggerTurnStartEffects(Action<string> logCallback)
    {
        OnTurnStart?.Invoke(this, logCallback);
    }
}