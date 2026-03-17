using UnityEngine;
using System.Collections.Generic;

public class CombatConsoleTester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("<color=yellow>--- ROZPOCZÊCIE TESTU WALKI ---</color>");

        // 1. Tworzymy silnego i szybkiego Gracza
        Player player = new Player("Bohater", maxHp: 50, attack: 15, defense: 5, speed: 20, dodgeChance: 0);

        // 2. Tworzymy paczkê danych dla wroga w pamiêci
        EnemyData goblinData = ScriptableObject.CreateInstance<EnemyData>();
        goblinData.enemyId = "Goblin";
        goblinData.maxHP = 20;
        goblinData.attack = 8;
        goblinData.defense = 2;
        goblinData.speed = 10;
        goblinData.xpReward = 15;

        Enemy enemy = new Enemy(goblinData);

        // 3. Inicjalizacja Mened¿era Walki
        CombatManager combatManager = new CombatManager();

        // Podpinamy komunikaty z mened¿era bezpoœrednio do konsoli Unity
        combatManager.OnCombatLog += (log) => Debug.Log($"<color=cyan>[Mened¿er]</color> {log}");
        combatManager.OnBattleEnded += (result) => Debug.Log($"<color=green>[Koniec Walki]</color> Wynik: {result}");

        // --- SYMULACJA WALKI ---

        // Start starcia (Gracz ma wiêcej speeda, wiêc pierwszy dostanie turê)
        combatManager.StartBattle(new List<Entity> { player, enemy });

        // TURA 1: Gracz bije
        ICombatCommand playerAttack = new AttackCommand(player, enemy, log => Debug.Log($"<color=orange>[Atak]</color> {log}"));
        combatManager.ExecuteTurnAction(playerAttack);
        Debug.Log($"Stan HP: Gracz {player.CurrentHP}/50 | Goblin {enemy.CurrentHP}/20");

        // TURA 2: Goblin oddaje
        ICombatCommand enemyAttack = new AttackCommand(enemy, player, log => Debug.Log($"<color=orange>[Atak]</color> {log}"));
        combatManager.ExecuteTurnAction(enemyAttack);
        Debug.Log($"Stan HP: Gracz {player.CurrentHP}/50 | Goblin {enemy.CurrentHP}/20");

        // TURA 3: Gracz dobija Goblina
        combatManager.ExecuteTurnAction(playerAttack);
        Debug.Log($"Stan HP po dobiciu: Gracz {player.CurrentHP}/50 | Goblin {enemy.CurrentHP}/20");

        Debug.Log("<color=yellow>--- KONIEC TESTU ---</color>");
    }
}