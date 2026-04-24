using System;

public class DefendCommand : ICombatCommand
{
    private Entity _caster;
    private Action<string> _combatLogCallback;

    public DefendCommand(Entity caster, Action<string> logCallback)
    {
        _caster = caster;
        _combatLogCallback = logCallback;
    }

    public void Execute()
    {
        // Prosta mechanika obrony: Logujemy akcj i oddajemy tur.
        // W pełniejszym systemie mona tu doda _caster.IsDefending = true
        _combatLogCallback?.Invoke($"{_caster.Name} przyjmuje postaw obronn, przygotowujc si na nadchodzcy cios!");
    }
}
