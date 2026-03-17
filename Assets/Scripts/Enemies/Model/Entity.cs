public abstract class Entity
{
    public int MaxHP { get; protected set; }
    public int CurrentHP { get; protected set; }

    public virtual void TakeDamage(int damage)
    {
        CurrentHP -= damage;

        if (CurrentHP < 0)
            CurrentHP = 0;
    }

    public bool IsAlive()
    {
        return CurrentHP > 0;
    }
}