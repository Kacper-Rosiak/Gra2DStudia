using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class QuestObjective
{
    public string objectiveDescription;
    public int currentAmount;
    public int targetAmount;
}

[System.Serializable]
public class QuestData
{
    public string questName;
    public List<QuestObjective> objectives;
    public int rewardGold;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action<QuestData> OnQuestProgressChanged;
    private List<QuestData> _aktywneMisje = new List<QuestData>();

    // REFERENCJA DO DZIENNIKA – DOPISZ TO:
    private QuestLogSzybki dziennikUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        GenerujTestowaMisje();
    }

    private void Start()
    {
        // Automatycznie szukamy dziennika na scenie, żebyś nie musiał nic przeciągać
        dziennikUI = FindFirstObjectByType<QuestLogSzybki>();
    }

    private void Update()
    {
        // PRZENIESIONE NASŁUCHIWANIE: Manager nigdy się nie wyłącza, więc zawsze usłyszy 'J'
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (dziennikUI != null)
            {
                Debug.Log("<color=yellow>[QuestManager]</color> Wykryto klawisz J! Przekazuję do UI.");
                dziennikUI.PrzełączDziennik();
            }
            else
            {
                // Fallback jeśli schowałeś obiekt w hierarchii pod wyłączonym rodzicem
                dziennikUI = Resources.FindObjectsOfTypeAll<QuestLogSzybki>()[0];
                if (dziennikUI != null) dziennikUI.PrzełączDziennik();
            }
        }
    }

    public List<QuestData> FindAllActiveAndCompletedQuests()
    {
        return _aktywneMisje;
    }

    private void GenerujTestowaMisje()
    {
        // === PODSTAWOWE MISJE (1 - 6) ===
        DodajQuest("Zlecenie na zielonoskórych", "Zabij gobliny w okolicy", 0, 6, 200);
        DodajQuest("Pierwsze zakupy", "Kup dowolny przedmiot u handlarza", 0, 1, 50);
        DodajQuest("Trening czyni mistrza", "Zdobądź 5 poziom doświadczenia", 0, 5, 300);
        DodajQuest("Eksploracja podziemi", "Przeszukaj zapomniane lochy", 0, 1, 500);
        DodajQuest("Ostatnia deska ratunku", "Użyj mikstury leczącej podczas walki", 0, 1, 100);
        DodajQuest("Łowca skarbów", "Przeszukaj ukryte skrzynie ze złotem", 0, 3, 250);
        DodajQuest("Pełna gotowość", "Załóż lepsze uzbrojenie w ekwipunku", 0, 1, 150);
        DodajQuest("Błyskawiczna egzekucja", "Zabij przeciwnika w mniej niż 3 ruchach", 0, 1, 400);
    }

    // Ta mini-funkcja zostaje bez zmian, pozwala dodawać misje jedną linijką
    private void DodajQuest(string nazwa, string opisCelu, int obecnyPostep, int wymaganyPostep, int zloto)
    {
        QuestData nowy = new QuestData();
        nowy.questName = nazwa;
        nowy.rewardGold = zloto;
        nowy.objectives = new List<QuestObjective>();

        QuestObjective cel = new QuestObjective();
        cel.objectiveDescription = opisCelu;
        cel.currentAmount = obecnyPostep;
        cel.targetAmount = wymaganyPostep;

        nowy.objectives.Add(cel);
        _aktywneMisje.Add(nowy);
    }
    public void ZwiekszPostepCelu(string opisCelu, int ilosc = 1)
    {
        foreach (QuestData misja in _aktywneMisje)
        {
            if (misja.objectives == null) continue;

            foreach (QuestObjective cel in misja.objectives)
            {
                // Sprawdzamy, czy opis celu z gry pasuje do opisu w misji
                if (cel.objectiveDescription == opisCelu)
                {
                    // Jeśli cel nie jest jeszcze skończony, dodajemy postęp
                    if (cel.currentAmount < cel.targetAmount)
                    {
                        cel.currentAmount += ilosc;
                        if (cel.currentAmount > cel.targetAmount) cel.currentAmount = cel.targetAmount;

                        Debug.Log($"<color=green>[QuestManager]</color> Postęp celu '{opisCelu}': {cel.currentAmount}/{cel.targetAmount}");

                        // !!! KLUCZOWY MOMENT !!!
                        // Odpalamy event, który automatycznie odświeża napisy w Twoim Dzienniku UI
                        // oraz daje sygnał do PlayerStats, żeby sprawdził, czy należy się kasa!
                        OnQuestProgressChanged?.Invoke(misja);
                    }
                }
            }
        }
    }
}