// CombatEnums.cs
// Definicje faz maszyny stanów
public enum BattleState
{
    Setup,          // Inicjalizacja walki i kolejki
    PlayerTurn,     // Oczekiwanie na akcję gracza
    EnemyTurn,      // AI wykonuje ruch
    Resolution,     // Obliczanie skutków akcji i sprawdzanie czy ktoś zginął
    End             // Zakończenie walki
}

// Możliwe wyniki starcia
public enum BattleResult
{
    Victory,
    Defeat,
    Escaped
}
