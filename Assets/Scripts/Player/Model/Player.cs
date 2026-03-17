// Player.cs
public class Player : Entity
{
    // Nadpisujemy w³aœciwoœæ bazow¹, aby Gracz móg³ mieæ unik
    public override int DodgeChance { get; protected set; }

    public Player(string name, int maxHp, int attack, int defense, int speed, int dodgeChance)
    {
        IsPlayer = true;
        Name = name;
        MaxHP = maxHp;
        CurrentHP = MaxHP;
        Attack = attack;
        Defense = defense;
        Speed = speed;
        DodgeChance = dodgeChance;
    }
}