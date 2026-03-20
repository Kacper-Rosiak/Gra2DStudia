using System;

public class UseAbilityCommand : ICombatCommand
{
    private Entity _caster;
    private Entity _target;
    private IAbilityStrategy _ability;
    private Action<string> _combatLogCallback;

    public UseAbilityCommand(Entity caster, Entity target, IAbilityStrategy ability, Action<string> logCallback)
    {
        _caster = caster;
        _target = target;
        _ability = ability;
        _combatLogCallback = logCallback;
    }

    public void Execute()
    {
        if (_ability != null)
        {
            _ability.Execute(_caster, _target, _combatLogCallback);
        }
    }
}