using System;

public interface IAbilityStrategy
{
    string AbilityName { get; }

    // Zdarzenie dla warstwy View (np. "Odtwórz cz¹steczki ognia", "Zagraj dŸwiêk tarczy")
    event Action<string> OnAbilityVisualsTriggered;

    void Execute(Entity caster, Entity target, Action<string> logCallback);
}