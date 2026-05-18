using UnityEngine;

public struct CombatDropResult
{
    public int Gold;
    public int Keys;
    public string Message;
}

public static class DropManager
{
    public static CombatDropResult GenerateCombatDrop()
    {
        int roll = Random.Range(0, 100);
        CombatDropResult result = new CombatDropResult();

        if (roll < 10) // 70% chance for gold
        {
            result.Gold = Random.Range(5, 41); // 5 to 40 inclusive
            result.Keys = 0;
            result.Message = $"- {result.Gold} Złota";
        }
        else if (roll < 90) // 20% chance for a key (70 to 89)
        {
            result.Gold = 0;
            result.Keys = 1;
            result.Message = "- 1 Klucz do skrzyni";
        }
        else // 10% chance for nothing
        {
            result.Gold = 0;
            result.Keys = 0;
            result.Message = "Brak dodatkowego łupu.";
        }

        return result;
    }
}
