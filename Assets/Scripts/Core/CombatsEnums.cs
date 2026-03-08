// CombatEnums.cs
// Definicje faz maszyny stanów
public enum BattleState
{
    Setup,          // Inicjalizacja walki i kolejki
    PlayerTurn,     // Oczekiwanie na akcjê gracza
    EnemyTurn,      // AI wykonuje ruch
    Resolution,     // Obliczanie skutków akcji i sprawdzanie czy ktoœ zgin¹³
    End             // Zakoñczenie walki
}

// Mo¿liwe wyniki starcia
public enum BattleResult
{
    Victory,
    Defeat,
    Escaped
}
