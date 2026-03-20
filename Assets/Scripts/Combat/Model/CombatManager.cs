// CombatManager.cs
using System;
using System.Collections.Generic;
using System.Linq;

public class CombatManager
{
    public BattleState CurrentState { get; private set; }

    public event Action<BattleState> OnStateChanged;
    public event Action<BattleResult> OnBattleEnded;
    public event Action<string> OnCombatLog;

    private List<Entity> _initiativeQueue; // Zmiana na Entity
    private int _currentTurnIndex;

    public void StartBattle(List<Entity> participants) // Zmiana na Entity
    {
        ChangeState(BattleState.Setup);

        _initiativeQueue = participants.OrderByDescending(p => p.Speed).ToList();
        _currentTurnIndex = 0;

        OnCombatLog?.Invoke("Starcie siê rozpoczyna!");
        ProceedToNextTurn();
    }

    private void ProceedToNextTurn()
    {
        if (CurrentState == BattleState.End) return;

        Entity activeEntity = _initiativeQueue[_currentTurnIndex];

        // Aktywacja efektów na starcie tury (np. obra¿enia z Burn)
        activeEntity.TriggerTurnStartEffects(log => OnCombatLog?.Invoke(log));

        // Jeœli postaæ umar³a od efektu (np. poparzenia), przejdŸ do podsumowania
        if (!activeEntity.IsAlive())
        {
            ResolveTurn();
            return;
        }

        // Obs³uga og³uszenia
        if (activeEntity.IsStunned)
        {
            OnCombatLog?.Invoke($"{activeEntity.Name} jest og³uszony i traci turê!");
            activeEntity.IsStunned = false; // Resetujemy status po pominiêciu tury
            ChangeState(BattleState.Resolution);
            ResolveTurn();
            return;
        }

        if (activeEntity.IsPlayer)
            ChangeState(BattleState.PlayerTurn);
        else
            ChangeState(BattleState.EnemyTurn);
    }

    public void ExecuteTurnAction(ICombatCommand command)
    {
        if (CurrentState != BattleState.PlayerTurn && CurrentState != BattleState.EnemyTurn)
            return;

        command.Execute();

        ChangeState(BattleState.Resolution);
        ResolveTurn();
    }

    private void ResolveTurn()
    {
        bool isPlayerAlive = _initiativeQueue.Any(p => p.IsPlayer && p.CurrentHP > 0);
        bool areEnemiesAlive = _initiativeQueue.Any(p => !p.IsPlayer && p.CurrentHP > 0);

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

        do
        {
            _currentTurnIndex = (_currentTurnIndex + 1) % _initiativeQueue.Count;
        }
        while (_initiativeQueue[_currentTurnIndex].CurrentHP <= 0);

        ProceedToNextTurn();
    }

    public void TryEscape(int successChancePercent)
    {
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
            ChangeState(BattleState.Resolution);
            ResolveTurn();
        }
    }

    private void EndBattle(BattleResult result)
    {
        ChangeState(BattleState.End);
        OnBattleEnded?.Invoke(result);
    }

    private void ChangeState(BattleState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }
}