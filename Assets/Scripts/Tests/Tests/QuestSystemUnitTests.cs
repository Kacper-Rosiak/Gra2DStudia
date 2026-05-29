using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

// ============================================================================
// SEKCKJA 1: KOMPLETNY ZESTAW TESTÓW SYSTEMU ZADAÑ (NUnit + EditMode)
// ============================================================================
[TestFixture]
public class QuestSystemUnitTests
{
    private PureQuestManager _questManager;
    private PurePlayerProfile _player;

    [SetUp]
    public void Setup()
    {
        // Przed ka¿dym testem tworzymy czystego gracza i nowy mened¿er zadañ
        _player = new PurePlayerProfile();
        _questManager = new PureQuestManager(_player);
    }

    // --- TEST 1: POSTÊP CELU ---

    [Test]
    public void OnEnemyKilled_MatchingTarget_IncreasesQuestObjectiveCounter()
    {
        // Given (Gracz ma aktywne zadanie na zabicie 3 Orków)
        PureQuest orcQuest = new PureQuest("Q01", "Zabij Orki", targetEnemyId: "Orc", requiredAmount: 3, rewardXP: 100, rewardGold: 50);
        _questManager.AcceptQuest(orcQuest);

        // When (Gracz zabija jednego Orka)
        _questManager.HandleEnemyKilled("Orc");

        // Then (Licznik powinien wzrosn¹æ z 0 na 1, ale zadanie nadal jest aktywne)
        Assert.AreEqual(1, orcQuest.CurrentAmount, "B£¥D: Licznik celu w zadaniu nie wzrós³ po zabiciu potwora!");
        Assert.AreEqual(PureQuestState.Active, orcQuest.State, "B£¥D: Zadanie ukoñczy³o siê przedwczeœnie!");
    }

    // --- TEST 2: NAGRODY I AUTOMATYCZNE ZAKOÑCZENIE ---

    [Test]
    public void OnEnemyKilled_ObjectiveMet_CompletesQuestAndGrantsRewards()
    {
        // Given (Zadanie wymaga zabicia tylko 1 bossa)
        PureQuest bossQuest = new PureQuest("Q02", "Zabij Bossa", targetEnemyId: "Dragon", requiredAmount: 1, rewardXP: 500, rewardGold: 1000);
        _questManager.AcceptQuest(bossQuest);

        // When (Zabijamy smoka)
        _questManager.HandleEnemyKilled("Dragon");

        // Then (Status zmienia siê na Completed, a gracz automatycznie dostaje XP i Z³oto)
        Assert.AreEqual(PureQuestState.Completed, bossQuest.State, "B£¥D: Zadanie nie zmieni³o statusu na Completed po spe³nieniu warunku!");
        Assert.AreEqual(500, _player.XP, "B£¥D: Gracz nie otrzyma³ punktów doœwiadczenia za zadanie!");
        Assert.AreEqual(1000, _player.Gold, "B£¥D: Gracz nie otrzyma³ z³ota za zadanie!");
    }

    // --- TEST 3: NIEZALE¯NOŒÆ ZADAÑ (RÓWNOLEG£E MISJE) ---

    [Test]
    public void HandleEnemyKilled_MultipleActiveQuests_UpdatesOnlyRelevantQuestsWithoutInterference()
    {
        // Given (Dwa ró¿ne, równoleg³e zadania)
        PureQuest wolfQuest = new PureQuest("Q03", "Zabij Wilka", targetEnemyId: "Wolf", requiredAmount: 1, rewardXP: 50, rewardGold: 10);
        PureQuest bearQuest = new PureQuest("Q04", "Zabij NiedŸwiedzia", targetEnemyId: "Bear", requiredAmount: 1, rewardXP: 100, rewardGold: 20);

        _questManager.AcceptQuest(wolfQuest);
        _questManager.AcceptQuest(bearQuest);

        // When (Gracz zabija tylko Wilka)
        _questManager.HandleEnemyKilled("Wolf");

        // Then (Zadanie na wilka siê koñczy i daje nagrody, ale zadanie na niedŸwiedzia pozostaje nienaruszone)
        Assert.AreEqual(PureQuestState.Completed, wolfQuest.State, "B£¥D: Zadanie na Wilka nie zosta³o ukoñczone!");
        Assert.AreEqual(PureQuestState.Active, bearQuest.State, "B£¥D: Zadanie na NiedŸwiedzia b³êdnie zareagowa³o na œmieræ innego potwora!");

        // Sprawdzamy, czy gracz dosta³ nagrodê TYLKO za wilka (50 XP, 10 Gold)
        Assert.AreEqual(50, _player.XP, "B£¥D: Przyznano b³êdn¹ iloœæ XP (mo¿e z obu zadañ naraz?)");
        Assert.AreEqual(10, _player.Gold, "B£¥D: Przyznano b³êdn¹ iloœæ z³ota!");
    }
}

// ============================================================================
// SEKCKJA 2: IZOLOWANA LOGIKA SYSTEMU ZADAÑ (Wymóg 5.0)
// ============================================================================

public enum PureQuestState { NotStarted, Active, Completed }

// Atrapa gracza, która odbiera nagrody z zadañ
public class PurePlayerProfile
{
    public int XP { get; private set; }
    public int Gold { get; private set; }

    public void AddReward(int xpAmount, int goldAmount)
    {
        XP += xpAmount;
        Gold += goldAmount;
    }
}

// Model pojedynczego zadania
public class PureQuest
{
    public string Id { get; private set; }
    public string Title { get; private set; }
    public PureQuestState State { get; private set; }

    // Warunki (Objective)
    public string TargetEnemyId { get; private set; }
    public int RequiredAmount { get; private set; }
    public int CurrentAmount { get; private set; }

    // Nagrody
    public int RewardXP { get; private set; }
    public int RewardGold { get; private set; }

    public bool IsObjectiveMet => CurrentAmount >= RequiredAmount;

    public PureQuest(string id, string title, string targetEnemyId, int requiredAmount, int rewardXP, int rewardGold)
    {
        Id = id;
        Title = title;
        State = PureQuestState.NotStarted;

        TargetEnemyId = targetEnemyId;
        RequiredAmount = requiredAmount;
        CurrentAmount = 0;

        RewardXP = rewardXP;
        RewardGold = rewardGold;
    }

    public void Start()
    {
        State = PureQuestState.Active;
    }

    public void ProcessKill(string enemyId)
    {
        if (State == PureQuestState.Active && enemyId == TargetEnemyId)
        {
            CurrentAmount++;
        }
    }

    public void Complete()
    {
        State = PureQuestState.Completed;
    }
}

// Mened¿er, który spina misje i rozsy³a sygna³y
public class PureQuestManager
{
    private List<PureQuest> _activeQuests = new List<PureQuest>();
    private PurePlayerProfile _player;

    public PureQuestManager(PurePlayerProfile player)
    {
        _player = player;
    }

    public void AcceptQuest(PureQuest quest)
    {
        quest.Start();
        _activeQuests.Add(quest);
    }

    // Ta funkcja symuluje wywo³anie zdarzenia OnEnemyKilled
    public void HandleEnemyKilled(string enemyId)
    {
        foreach (var quest in _activeQuests)
        {
            // Przekazujemy informacjê o zabiciu do misji
            quest.ProcessKill(enemyId);

            // AUTOMATYCZNE ROZLICZANIE (Wymóg zadania: nagrody po spe³nieniu warunków)
            if (quest.State == PureQuestState.Active && quest.IsObjectiveMet)
            {
                quest.Complete();
                _player.AddReward(quest.RewardXP, quest.RewardGold);
            }
        }
    }
}