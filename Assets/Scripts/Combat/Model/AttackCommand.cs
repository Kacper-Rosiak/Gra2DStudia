// AttackCommand.cs
using System;

public class AttackCommand : ICombatCommand
{
    private CombatEntity _attacker;
    private CombatEntity _target;
    private Action<string> _combatLogCallback; // Event do informowania UI o przebiegu ciosu

    public AttackCommand(CombatEntity attacker, CombatEntity target, Action<string> logCallback)
    {
        _attacker = attacker;
        _target = target;
        _combatLogCallback = logCallback;
    }

    public void Execute()
    {
        Random rnd = new Random();

        // 1. Logika Uniku (Dodge)
        if (rnd.Next(0, 100) < _target.DodgeChance)
        {
            _combatLogCallback?.Invoke($"{_attacker.Name} atakuje, ale {_target.Name} wykonuje unik!");
            return; // Przerwanie ciosu
        }

        // 2. Logika Obra¿eñ (Zabezpieczenie: cios zadaje minimum 1 pkt obra¿eñ)
        int damage = Math.Max(1, _attacker.Attack - _target.Defense);

        // 3. Zaaplikowanie obra¿eñ i wys³anie logu
        _target.TakeDamage(damage);
        _combatLogCallback?.Invoke($"{_attacker.Name} trafia! Zadaje {damage} obra¿eñ postaci {_target.Name}.");
    }
}