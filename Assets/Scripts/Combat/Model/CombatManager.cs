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

        OnCombatLog?.Invoke("Starcie si� rozpoczyna!");
        ProceedToNextTurn();
    }

    private void ProceedToNextTurn()
    {
        if (CurrentState == BattleState.End) return;

        Entity activeEntity = _initiativeQueue[_currentTurnIndex];

        // 1. Aktywacja efekt�w na starcie tury (np. trucizna z klasy Assassin)
        activeEntity.TriggerTurnStartEffects(log => OnCombatLog?.Invoke(log));

        // Je�li posta� umar�a od efektu (np. poparzenia), przejd� do podsumowania
        if (!activeEntity.IsAlive())
        {
            ChangeState(BattleState.Resolution);
            ResolveTurn();
            return;
        }

        // 2. Obs�uga og�uszenia (np. z klasy Warrior)
        if (activeEntity.IsStunned)
        {
            OnCombatLog?.Invoke($"{activeEntity.Name} jest og�uszony i traci tur�!");
            activeEntity.IsStunned = false; // Resetujemy status po pomini�ciu tury

            ChangeState(BattleState.Resolution);
            ResolveTurn();
            return;
        }

        // 3. Rozdanie tur
        if (activeEntity.IsPlayer)
        {
            ChangeState(BattleState.PlayerTurn);
            // Tutaj system czeka. Nic si� nie dzieje, dop�ki Gracz nie kliknie przycisku ataku.
        }
        else
        {
            ChangeState(BattleState.EnemyTurn);
            // Automatyczny ruch przeciwnika (Sztuczna Inteligencja dla 1v1)
            ExecuteEnemyTurn(activeEntity);
        }
    }

    // --- NOWA METODA: Skrypt tury wroga dla walk 1v1 ---
    private async void ExecuteEnemyTurn(Entity enemy)
    {
        // W walce 1v1 celem jest po prostu jedyny gracz na licie inicjatywy
        Entity playerTarget = _initiativeQueue.Find(e => e.IsPlayer);

        if (playerTarget != null && playerTarget.IsAlive())
        {
            // Opónienie 5 sekund przed ruchem wroga
            await System.Threading.Tasks.Task.Delay(5000);

            // Sprawdzenie czy walka nadal trwa po odczekaniu
            if (CurrentState != BattleState.EnemyTurn) return;

            // Tworzymy komend zwykego ataku dla wroga
            ICombatCommand enemyAttack = new AttackCommand(enemy, playerTarget, log => OnCombatLog?.Invoke($"<color=red>[WROGI ATAK]</color> {log}"));

            // ExecuteTurnAction samo zajmie si wywoaniem komendy i popchniciem kolejki dalej
            ExecuteTurnAction(enemyAttack);
        }
        else
        {
            // Zabezpieczenie, gdyby gracza nie byo (np. zgin wczeniej)
            ChangeState(BattleState.Resolution);
            ResolveTurn();
        }
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
            OnCombatLog?.Invoke("Uda�o ci si� uciec z pola walki!");
            EndBattle(BattleResult.Escaped);
        }
        else
        {
            OnCombatLog?.Invoke("Pr�ba ucieczki nie powiod�a si�! Tracisz tur�.");
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