using UnityEngine;
using System.Collections.Generic;

public class CombatConsoleTester : MonoBehaviour
{
    [Header("Przeci¹gnij tutaj klasê postaci (np. AssassinData) z folderu Data")]
    public PlayerClassData testPlayerClass;

    void Start()
    {
        if (testPlayerClass == null) return;

        Debug.Log($"<color=yellow>--- WALKA AUTOMATYCZNA: KLASA {testPlayerClass.className.ToUpper()} ---</color>");

        PlayerStats playerStats = new PlayerStats(testPlayerClass);

        IAbilityStrategy abilityStrategy = null;
        if (testPlayerClass.className == "Warrior") abilityStrategy = new WarriorAbilityStrategy();
        else if (testPlayerClass.className == "Mage") abilityStrategy = new MageAbilityStrategy();
        else if (testPlayerClass.className == "Assassin") abilityStrategy = new AssassinAbilityStrategy();

        Player player = new Player("Bohater", playerStats, abilityStrategy);

        EnemyData goblinData = ScriptableObject.CreateInstance<EnemyData>();
        goblinData.enemyId = "Goblin";
        goblinData.maxHP = 30; // Zwiêkszy³em HP goblina, ¿eby prze¿y³ wiêcej ni¿ 1 cios
        goblinData.attack = 8;
        goblinData.defense = 2;
        goblinData.speed = 10;
        goblinData.xpReward = 15;

        Enemy enemy = new Enemy(goblinData);

        CombatManager combatManager = new CombatManager();
        combatManager.OnCombatLog += (log) => Debug.Log($"<color=cyan>[Log]</color> {log}");
        combatManager.OnBattleEnded += (result) => Debug.Log($"<color=green>[Koniec Walki]</color> Wynik: {result}");

        // --- SYMULACJA WALKI ---

        // 1. Start starcia (Mened¿er ustawi kolejkê i da turê najszybszemu, pewnie Graczowi)
        combatManager.StartBattle(new List<Entity> { player, enemy });

        // 2. GRACZ KLIKA: Zwyk³y Atak
        // Po wywo³aniu tej metody, Mened¿er sam przeliczy obra¿enia gracza, a potem AUTOMATYCZNIE odpali turê Goblina!
        ICombatCommand normalAttack = new AttackCommand(player, enemy, log => Debug.Log($"<color=orange>[Atak Gracza]</color> {log}"));
        combatManager.ExecuteTurnAction(normalAttack);

        // 3. GRACZ KLIKA: Umiejêtnoœæ Specjalna
        // Znowu: Gracz bije (np. Mag rzuca kulê ognia, podpalaj¹c Goblina), a Goblin po otrzymaniu ciosu automatycznie oddaje.
        ICombatCommand specialAbility = new UseAbilityCommand(player, enemy, player.SpecialAbility, log => Debug.Log($"<color=magenta>[Zdolnoœæ Gracza]</color> {log}"));
        combatManager.ExecuteTurnAction(specialAbility);

        Debug.Log($"<color=yellow>--- KONIEC SYMULACJI ---</color>");
    }
}