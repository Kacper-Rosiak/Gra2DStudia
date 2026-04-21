using UnityEngine;

public class CombatTester : MonoBehaviour
{
    [Header("Referencja do Twojego Kontrolera")]
    public CombatController controller;

    private FakeEntity testPlayer; // Zmienna, żebyśmy mieli dostęp do gracza później

    void Start()
    {
        // Odliczamy 1 sekundę po uruchomieniu, żeby wszystko w Unity zdążyło się załadować
        Invoke("StartTestBattle", 1f);
    }

    public void StartTestBattle()
    {
        if (controller == null)
        {
            Debug.LogError("Tester: Nie przypisałeś CombatController w Inspektorze!");
            return;
        }

        Debug.Log("Tester: Inicjalizacja walki testowej...");

        // 1. Tworzymy testowego gracza
        testPlayer = new FakeEntity();
        testPlayer.Name = "Bohater";
        testPlayer.SetStats(100, true); // 100 HP, to jest gracz

        // 2. Tworzymy testowego wroga
        FakeEntity enemy = new FakeEntity();
        enemy.Name = "Zły Goblin";
        enemy.SetStats(80, false); // 80 HP, to nie jest gracz

        // 3. Przekazujemy ich do Twojego systemu
        controller.SetupBattle(testPlayer, enemy);

        // 4. PLANUJEMY TEST OBRAŻEŃ: Za 5 sekund zabierzemy graczowi 5 HP
        Invoke("DamageTest", 5f);
    }

    void DamageTest()
    {
        if (testPlayer != null)
        {
            Debug.Log("Tester: Minęło 5s! Zadaję testowe -5 HP graczowi.");

            // Wywołujemy metodę z Entity.cs - to powinno zaktualizować pasek HP w Twoim UI
            testPlayer.TakeDamage(5);
        }
    }
}

// KLASA POMOCNICZA: Pozwala stworzyć obiekt Entity, mimo że jest ono abstrakcyjne
public class FakeEntity : Entity
{
    public void SetStats(int hp, bool isPlayer)
    {
        MaxHP = hp;
        CurrentHP = hp;
        IsPlayer = isPlayer;

        // Statystyki walki na sztywno, żeby skrypty kolegów nie wywalały błędów
        Attack = 10;
        Defense = 5;
        Speed = 10;
    }
}