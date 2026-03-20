// Player.cs
public class Player : Entity
{
    private PlayerStats _stats;

    // Nadpisujemy w³aœciwoœci bazowe, aby dynamicznie ci¹gnê³y dane ze statystyk gracza
    public override int MaxHP => _stats.MaxHP;
    public override int CurrentHP => _stats.CurrentHP;
    public override int Attack => _stats.Attack;
    public override int Defense => _stats.Defense;
    public override int Speed => _stats.Speed;
    public override int DodgeChance => _stats.DodgeChance;
    public IAbilityStrategy SpecialAbility { get; private set; }

    // Konstruktor przyjmuje referencjê do statystyk z PlayerManager
    public Player(string name, PlayerStats stats, IAbilityStrategy ability)
    {
        IsPlayer = true;
        Name = name;
        _stats = stats;
        SpecialAbility = ability;
    }

    // Przekazujemy otrzymane obra¿enia bezpoœrednio do PlayerStats
    public override void TakeDamage(int damage)
    {
        _stats.TakeDamage(damage);
    }
}