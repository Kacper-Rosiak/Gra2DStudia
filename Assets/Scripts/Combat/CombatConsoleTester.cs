using UnityEngine;
using System.Collections.Generic;

public class CombatConsoleTester : MonoBehaviour
{
    [Header("Przeci¹gnij tutaj WarriorData lub MageData z folderu Data")]
    public PlayerClassData testPlayerClass;

    void Start()
    {
        if (testPlayerClass == null)
        {
            Debug.LogError("Brak klasy gracza! Przeci¹gnij plik (np. WarriorData) do pola Test Player Class w Inspektorze.");
            return;
        }

        Debug.Log($"<color=yellow>--- ROZPOCZÊCIE TESTU WALKI: KLASA {testPlayerClass.className.ToUpper()} ---</color>");

        // 1. Inicjalizacja statystyk z obiektu Data-Driven
        PlayerStats playerStats = new PlayerStats(testPlayerClass);

        // 2. Wybór strategii na podstawie nazwy klasy
        IAbilityStrategy abilityStrategy = null;
        if (testPlayerClass.className == "Warrior")
            abilityStrategy = new WarriorAbilityStrategy();
        else if (testPlayerClass.className == "Mage")
            abilityStrategy = new MageAbilityStrategy();

        // 3. Tworzymy Gracza z nowym, wymaganym parametrem strategii (TO NAPRAWIA TWÓJ B£¥D)
        Player player = new Player("Bohater", playerStats, abilityStrategy);

        // 4. Tworzymy paczkê danych dla wroga w pamiêci
        EnemyData goblinData = ScriptableObject.CreateInstance<EnemyData>();
        goblinData.enemyId = "Goblin";
        goblinData.maxHP = 20;
        goblinData.attack = 8;
        goblinData.defense = 2;
        goblinData.speed = 10;
        goblinData.xpReward = 15;

        Enemy enemy = new Enemy(goblinData);

        // 5. Inicjalizacja Mened¿era Walki
        CombatManager combatManager = new CombatManager();
        combatManager.OnCombatLog += (log) => Debug.Log($"<color=cyan>[Mened¿er]</color> {log}");
        combatManager.OnBattleEnded += (result) => Debug.Log($"<color=green>[Koniec Walki]</color> Wynik: {result}");

        // --- SYMULACJA WALKI ---

        combatManager.StartBattle(new List<Entity> { player, enemy });

        // TURA 1: Gracz u¿ywa ZWYK£EGO ataku
        ICombatCommand normalAttack = new AttackCommand(player, enemy, log => Debug.Log($"<color=orange>[Zwyk³y Atak]</color> {log}"));
        combatManager.ExecuteTurnAction(normalAttack);

        // TURA 2: Goblin oddaje
        ICombatCommand enemyAttack = new AttackCommand(enemy, player, log => Debug.Log($"<color=red>[Atak Wroga]</color> {log}"));
        combatManager.ExecuteTurnAction(enemyAttack);

        // TURA 3: Gracz u¿ywa ZDOLNOŒCI SPECJALNEJ (Z naszego nowego systemu!)
        ICombatCommand specialAbility = new UseAbilityCommand(player, enemy, player.SpecialAbility, log => Debug.Log($"<color=magenta>[Umiejêtnoœæ]</color> {log}"));
        combatManager.ExecuteTurnAction(specialAbility);

        Debug.Log($"<color=yellow>--- KONIEC SYMULACJI ---</color>");
    }
}