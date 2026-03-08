// CombatManager.cs
using System;
using System.Collections.Generic;
using System.Linq;

public class CombatManager
{
    public BattleState CurrentState { get; private set; }

    // Zdarzenia (Events) dla warstwy View. UI bêdzie ich "s³uchaæ".
    public event Action<BattleState> OnStateChanged;
    public event Action<BattleResult> OnBattleEnded;
    public event Action<string> OnCombatLog;

    private List<CombatEntity> _initiativeQueue;
    private int _currentTurnIndex; // Œledzi, czyja jest teraz tura

    // --- FAZA: SETUP ---
    public void StartBattle(List<CombatEntity> participants)
    {
        ChangeState(BattleState.Setup);

        // Sortowanie wszystkich postaci wg Szybkoœci (od najwiêkszej do najmniejszej)
        _initiativeQueue = participants.OrderByDescending(p => p.Speed).ToList();
        _currentTurnIndex = 0;

        OnCombatLog?.Invoke("Starcie siê rozpoczyna!");
        ProceedToNextTurn();
    }

    // --- FAZA: PLAYER / ENEMY TURN ---
    private void ProceedToNextTurn()
    {
        if (CurrentState == BattleState.End) return;

        CombatEntity activeEntity = _initiativeQueue[_currentTurnIndex];

        // Prze³¹czamy stan maszyny w zale¿noœci od typu postaci
        if (activeEntity.IsPlayer)
            ChangeState(BattleState.PlayerTurn);
        else
            ChangeState(BattleState.EnemyTurn);
    }

    // Odbiera komendy od UI (Gdy klikniesz przycisk) lub od AI
    public void ExecuteTurnAction(ICombatCommand command)
    {
        if (CurrentState != BattleState.PlayerTurn && CurrentState != BattleState.EnemyTurn)
            return; // Blokada przed wykonaniem akcji w z³ym momencie

        // Wykonanie zamkniêtej logiki (np. AttackCommand)
        command.Execute();

        // Po akcji maszyna bezwarunkowo wchodzi w fazê rozstrzygania
        ChangeState(BattleState.Resolution);
        ResolveTurn();
    }

    // --- FAZA: RESOLUTION (Sprawdzanie wyników) ---
    private void ResolveTurn()
    {
        // Sprawdzamy czy frakcje maj¹ jeszcze ¿yj¹cych reprezentantów
        bool isPlayerAlive = _initiativeQueue.Any(p => p.IsPlayer && p.CurrentHP > 0);
        bool areEnemiesAlive = _initiativeQueue.Any(p => !p.IsPlayer && p.CurrentHP > 0);

        // Wykrywanie koñca gry
        if (!isPlayerAlive)
        {
            EndBattle(BattleResult.Defeat);
            return;
        }

        if (!areEnemiesAlive)
        {
            EndBattle(BattleResult.Victory);
            return;
        }

        // Jeœli walka trwa, znajdŸ nastêpn¹ w kolejce ¯YW¥ postaæ
        do
        {
            _currentTurnIndex = (_currentTurnIndex + 1) % _initiativeQueue.Count;
        }
        while (_initiativeQueue[_currentTurnIndex].CurrentHP <= 0); // Pomijamy trupy

        ProceedToNextTurn(); // Pêtla siê zamyka, zaczynamy now¹ turê
    }

    // --- MECHANIKA UCIECZKI ---
    public void TryEscape(int successChancePercent)
    {
        // Tylko gracz mo¿e próbowaæ ucieczki podczas swojej tury
        if (CurrentState != BattleState.PlayerTurn) return;

        Random rnd = new Random();
        if (rnd.Next(0, 100) < successChancePercent)
        {
            OnCombatLog?.Invoke("Uda³o ci siê uciec z pola walki!");
            EndBattle(BattleResult.Escaped);
        }
        else
        {
            OnCombatLog?.Invoke("Próba ucieczki nie powiod³a siê! Tracisz turê.");
            // Utrata tury - przejœcie do weryfikacji
            ChangeState(BattleState.Resolution);
            ResolveTurn();
        }
    }

    // --- FAZA: END ---
    private void EndBattle(BattleResult result)
    {
        ChangeState(BattleState.End);
        OnBattleEnded?.Invoke(result); // Wys³anie wyniku do innych systemów (np. Lootu)
    }

    // Wewnêtrzna metoda do hermetycznej zmiany stanu i informowania UI
    private void ChangeState(BattleState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }
}