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

        // 1. Logika Uniku (Dodge)
        if (_target != null && _target.DodgeChance > 0 && rnd.Next(0, 100) < _target.DodgeChance)
        {
            _combatLogCallback?.Invoke($"{_attacker.Name} atakuje, ale {_target.Name} wykonuje unik!");
            return;
        }

        // 2. Logika Obrażeń
        int damage = Math.Max(1, _attacker.Attack - (_target != null ? _target.Defense : 0));

        // 3. Zaaplikowanie obrażeń i wysłanie logu
        if (_target != null)
        {
            _target.TakeDamage(damage);
            _combatLogCallback?.Invoke($"{_attacker.Name} atakuje za {damage} pkt obrażeń w {_target.Name}!");
        }
    }
}
