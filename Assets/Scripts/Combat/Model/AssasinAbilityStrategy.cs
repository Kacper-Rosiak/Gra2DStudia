using System;

public class AssassinAbilityStrategy : IAbilityStrategy
{
    public string AbilityName => "Zatruty Sztylet";

    public event Action<string> OnAbilityVisualsTriggered;

    public void Execute(Entity caster, Entity target, Action<string> logCallback)
    {
        // Sygna³ dla widoku (np. zielony rozb³ysk)
        OnAbilityVisualsTriggered?.Invoke("PoisonDagger_VFX");

        // Cios precyzyjny: obra¿enia skaluj¹ siê z Atakiem oraz po³ow¹ Szybkoœci
        int damage = Math.Max(1, (caster.Attack + (caster.Speed / 2)) - target.Defense);
        target.TakeDamage(damage);

        logCallback?.Invoke($"{caster.Name} atakuje z cienia u¿ywaj¹c {AbilityName}! Zadaje {damage} obra¿eñ postaci {target.Name}.");

        // Nak³adanie statusu trucizny (podobnie jak podpalenie u Maga)
        target.OnTurnStart += ApplyPoisonDamage;
        logCallback?.Invoke($"{target.Name} zostaje zatruty!");
    }

    private void ApplyPoisonDamage(Entity affectedEntity, Action<string> logCallback)
    {
        int poisonDamage = 4;
        affectedEntity.TakeDamage(poisonDamage);
        logCallback?.Invoke($"<color=green>[Trucizna]</color> {affectedEntity.Name} traci {poisonDamage} HP z powodu dzia³ania jadu!");
    }
}