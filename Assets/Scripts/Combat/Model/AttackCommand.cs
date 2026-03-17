// AttackCommand.cs
using System;

public class AttackCommand : ICombatCommand
{
    private Entity _attacker;
    private Entity _target;
    private Action<string> _combatLogCallback;

    public AttackCommand(Entity attacker, Entity target, Action<string> logCallback)
    {
        _attacker = attacker;
        _target = target;
        _combatLogCallback = logCallback;
    }

    public void Execute()
    {
        Random rnd = new Random();

        // 1. Logika Uniku (Dodge) - uruchomi siê tylko jeœli DodgeChance > 0
        if (_target.DodgeChance > 0 && rnd.Next(0, 100) < _target.DodgeChance)
        {
            _combatLogCallback?.Invoke($"{_attacker.Name} atakuje, ale {_target.Name} wykonuje unik!");
            return;
        }

        // 2. Logika Obra¿eñ (Pancerz jest uwzglêdniany tylko tutaj)
        int damage = Math.Max(1, _attacker.Attack - _target.Defense);

        // 3. Zaaplikowanie obra¿eñ i wys³anie logu
        _target.TakeDamage(damage);
        _combatLogCallback?.Invoke($"{_attacker.Name} trafia! Zadaje {damage} obra¿eñ postaci {_target.Name}.");
    }
}