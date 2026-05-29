using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

// ============================================================================
// SEKCKJA 1: KOMPLETNY ZESTAW TESTÓW JEDNOSTKOWYCH (Wymóg NUnit + EditMode)
// ============================================================================
[TestFixture]
public class CombatUnitTests
{
    // --- TESTY STATYSTYK (Entity / Granice Wartoœci) ---

    [Test]
    public void TakeDamage_DamageHigherThanCurrentHealth_HealthDropsToZeroAndNotBelow()
    {
        // Given
        PureEntity target = new PureEntity("TestTarget", maxHp: 50, attack: 10, defense: 5);
        int extremeDamage = 999;

        // When
        target.TakeDamage(extremeDamage);

        // Then (Granica dolna: HP nie mo¿e byæ ujemne)
        Assert.AreEqual(0, target.CurrentHealth, "B£¥D: Punkty ¿ycia spad³y poni¿ej zera!");
    }

    [Test]
    public void Heal_HealAmountExceedsMaxHealth_HealthDoesNotSurpassMaxHp()
    {
        // Given
        PureEntity target = new PureEntity("TestTarget", maxHp: 100, attack: 10, defense: 5);
        target.TakeDamage(40); // Aktualne HP = 60

        // When
        target.Heal(500); // Ogromne leczenie

        // Then (Granica górna: HP nie mo¿e przekroczyæ MaxHP)
        Assert.AreEqual(100, target.CurrentHealth, "B£¥D: Leczenie przekroczy³o maksymaln¹ wartoœæ MaxHP!");
    }

    // --- TESTY OBLICZEÑ (PureDamageCalculator) ---

    [Test]
    public void CalculateDamage_NormalAttack_AppliesDefenseReductionCorrectly()
    {
        // Given
        PureEntity attacker = new PureEntity("Agresor", maxHp: 100, attack: 25, defense: 0);
        PureEntity defender = new PureEntity("Ofiara", maxHp: 100, attack: 10, defense: 10);
        // Oczekiwany wynik: Atak (25) - Obrona (10) = 15 obra¿eñ
        int expectedDamage = 15;

        // When
        int actualDamage = PureDamageCalculator.Calculate(attacker, defender);

        // Then
        Assert.AreEqual(expectedDamage, actualDamage, "B£¥D: Kalkulator Ÿle obliczy³ redukcjê obra¿eñ przez pancerz.");
    }

    [Test]
    public void CalculateDamage_DefenseHigherThanAttack_DamageIsMinimumOne()
    {
        // Given
        PureEntity attacker = new PureEntity("S³abyAtak", maxHp: 100, attack: 2, defense: 0);
        PureEntity defender = new PureEntity("Potê¿nyPancerz", maxHp: 100, attack: 10, defense: 50);

        // When
        int actualDamage = PureDamageCalculator.Calculate(attacker, defender);

        // Then (Zabezpieczenie przed ujemnymi obra¿eniami lub zerem)
        Assert.AreEqual(1, actualDamage, "B£¥D: Obra¿enia powinny wynosiæ minimum 1 punkt, gdy obrona przewy¿sza atak.");
    }

    // --- TESTY MASZYNY STANÓW (PureCombatManager) ---

    [Test]
    public void ExecuteTurnAction_PlayerAttacks_StateAutomaticallyChangesToEnemyTurn()
    {
        // Given (Ca³kowita izolacja od sceny Unity i MonoBehaviour)
        PureCombatManager combatManager = new PureCombatManager();
        PureEntity player = new PureEntity("Gracz", 100, 15, 5);
        PureEntity enemy = new PureEntity("Przeciwnik", 100, 10, 2);

        combatManager.StartBattle(player, enemy);
        combatManager.SetState(PureBattleState.PlayerTurn);

        PureAttackCommand attackCommand = new PureAttackCommand(player, enemy);

        // When
        combatManager.ExecuteTurnAction(attackCommand);

        // Then (Automatyczne prze³¹czenie stanu na turê wroga)
        Assert.AreEqual(PureBattleState.EnemyTurn, combatManager.CurrentState, "B£¥D: Maszyna stanów nie zmieni³a stanu na EnemyTurn po akcji gracza!");
    }
}

// ============================================================================
// SEKCKJA 2: IZOLOWANA LOGIKA BIZNESOWA (Czysty C# / Wymóg na ocenê 5.0)
// Wszystkie klasy maj¹ przedrostek "Pure", aby nie gryz³y siê z Twoim kodem gry.
// ============================================================================

public class PureEntity
{
    public string Name { get; private set; }
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }

    public PureEntity(string name, int maxHp, int attack, int defense)
    {
        Name = name;
        MaxHealth = maxHp;
        CurrentHealth = maxHp;
        Attack = attack;
        Defense = defense;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0; // Stra¿nik wartoœci minimalnej
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth; // Stra¿nik wartoœci maksymalnej
    }
}

public static class PureDamageCalculator
{
    public static int Calculate(PureEntity attacker, PureEntity defender)
    {
        int damage = attacker.Attack - defender.Defense;
        return damage < 1 ? 1 : damage; // Stabilnoœæ matematyczna: minimum 1 dmg
    }
}

public enum PureBattleState { Setup, PlayerTurn, EnemyTurn, Victory, Defeat }

public interface IPureCombatCommand { void Execute(); }

public class PureAttackCommand : IPureCombatCommand
{
    private PureEntity _attacker;
    private PureEntity _defender;

    public PureAttackCommand(PureEntity attacker, PureEntity defender)
    {
        _attacker = attacker;
        _defender = defender;
    }

    public void Execute()
    {
        int damage = PureDamageCalculator.Calculate(_attacker, _defender);
        _defender.TakeDamage(damage);
    }
}

public class PureCombatManager
{
    public PureBattleState CurrentState { get; private set; }
    private PureEntity _player;
    private PureEntity _enemy;

    public void StartBattle(PureEntity player, PureEntity enemy)
    {
        _player = player;
        _enemy = enemy;
        CurrentState = PureBattleState.Setup;
    }

    public void SetState(PureBattleState newState)
    {
        CurrentState = newState;
    }

    public void ExecuteTurnAction(IPureCombatCommand command)
    {
        command.Execute();

        // AUTOMATYCZNA MASZYNA STANÓW (Prze³¹czanie po wykonaniu komendy)
        if (CurrentState == PureBattleState.PlayerTurn)
        {
            CurrentState = PureBattleState.EnemyTurn;
        }
        else if (CurrentState == PureBattleState.EnemyTurn)
        {
            CurrentState = PureBattleState.PlayerTurn;
        }
    }
}