using System;

public class WarriorAbilityStrategy : IAbilityStrategy
{
    public string AbilityName => "Uderzenie Tarcz¹";

    public event Action<string> OnAbilityVisualsTriggered;

    public void Execute(Entity caster, Entity target, Action<string> logCallback)
    {
        // Model-View: Wywo³anie zdarzenia dla widoku
        OnAbilityVisualsTriggered?.Invoke("ShieldBash_VFX");

        // Logika: Obra¿enia = Atak + Obrona atakuj¹cego - Obrona celu
        int damage = Math.Max(1, (caster.Attack + caster.Defense) - target.Defense);
        target.TakeDamage(damage);

        logCallback?.Invoke($"{caster.Name} u¿ywa {AbilityName}! Zadaje {damage} obra¿eñ postaci {target.Name}.");

        // Szansa na og³uszenie (np. 50%)
        Random rnd = new Random();
        if (rnd.Next(0, 100) < 50)
        {
            target.IsStunned = true;
            logCallback?.Invoke($"{target.Name} zostaje og³uszony i traci nastêpn¹ turê!");
        }
    }
}