// Entity.cs
public abstract class Entity
{
    public string Name { get; set; }
    public bool IsPlayer { get; set; }

    public int MaxHP { get; protected set; }
    public int CurrentHP { get; protected set; }
    public int Attack { get; protected set; }
    public int Defense { get; protected set; }
    public int Speed { get; protected set; }

    // Domyœlnie 0. Tylko klasa Gracza bêdzie to zmieniaæ.
    public virtual int DodgeChance { get; protected set; } = 0;

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
}