using System;

public class MageAbilityStrategy : IAbilityStrategy
{
    public string AbilityName => "Kula Ognia";

    public event Action<string> OnAbilityVisualsTriggered;

    public void Execute(Entity caster, Entity target, Action<string> logCallback)
    {
        OnAbilityVisualsTriggered?.Invoke("Fireball_VFX");

        // Magia ignoruje pancerz (omijamy target.Defense)
        int magicDamage = Math.Max(1, caster.Attack * 2);
        target.TakeDamage(magicDamage);

        logCallback?.Invoke($"{caster.Name} rzuca {AbilityName}! Magia ignoruje pancerz, zadaj¹c {magicDamage} obra¿eñ {target.Name}.");

        // OCP: Dodajemy status podpalenia za pomoc¹ anonimowej akcji (lub delegatu) podpiêtej pod pocz¹tek tury celu
        target.OnTurnStart += ApplyBurnDamage;
        logCallback?.Invoke($"{target.Name} zaczyna p³on¹æ!");
    }

    private void ApplyBurnDamage(Entity affectedEntity, Action<string> logCallback)
    {
        int burnDamage = 3;
        affectedEntity.TakeDamage(burnDamage);
        logCallback?.Invoke($"{affectedEntity.Name} otrzymuje {burnDamage} obra¿eñ od poparzenia!");
    }
}