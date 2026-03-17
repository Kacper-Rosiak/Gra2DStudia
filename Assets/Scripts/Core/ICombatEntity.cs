public interface ICombatEntity
{
    int CurrentHP { get; }
    int Speed { get; }

    void TakeDamage(int damage);
    bool IsAlive();
}