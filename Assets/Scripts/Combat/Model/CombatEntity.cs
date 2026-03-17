// CombatEntity.cs
public class CombatEntity
{
    public string Name { get; set; }
    public bool IsPlayer { get; set; } // Odró¿nia gracza od przeciwników

    // Podstawowe punkty ¿ycia
    public int MaxHP { get; set; }
    public int CurrentHP { get; set; }

    // Statystyki do obliczeñ
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }       // Decyduje o miejscu w kolejce inicjatywy
    public int DodgeChance { get; set; } // Szansa na unikniêcie ciosu w % (np. 15)

    // Metoda hermetyzuj¹ca utratê HP
    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        if (CurrentHP < 0)
        {
            CurrentHP = 0; // Zabezpieczenie przed ujemnym HP
        }
    }
}
