using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogSzybki : MonoBehaviour
{
    [Header("UI Kontenery")]
    [SerializeField] private GameObject oknoDziennika;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject tekstPrefab;

    private void Start()
    {
        // Na starcie chowamy, ale bezpiecznie
        if (oknoDziennika != null) oknoDziennika.SetActive(false);
    }

    // Funkcja odpalana przez QuestManagera po kliknięciu J
    public void PrzełączDziennik()
    {
        if (oknoDziennika == null) return;

        bool czyAktywne = !oknoDziennika.activeSelf;
        oknoDziennika.SetActive(czyAktywne);

        if (czyAktywne)
        {
            Time.timeScale = 0f; // Pauza gry
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Odpalamy rysowanie misji
            OdswiezListeMisji();
        }
        else
        {
            Time.timeScale = 1f; // Odpauzowanie gry
        }
    }

    public void OdswiezListeMisji()
    {
        // Na wszelki wypadek szukamy contentu, jeśli Unity zgubiło referencję przez wyłączenie obiektu
        if (contentParent == null)
        {
            contentParent = transform.Find("ScrollRect/Viewport/Content");
            if (contentParent == null) contentParent = GetComponentInChildren<VerticalLayoutGroup>()?.transform;
        }

        if (contentParent == null)
        {
            Debug.LogError("[QuestLog] Nie znalazłem kontenera Content! Upewnij się, że jest dobrze przypisany w Inspektorze.");
            return;
        }

        // Czyszczenie starych napisów
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogError("[QuestLog] Brak QuestManagera na scenie!");
            return;
        }

        // Pobranie misji
        List<QuestData> wszystkieMisje = QuestManager.Instance.FindAllActiveAndCompletedQuests();
        if (wszystkieMisje == null) return;

        foreach (QuestData misja in wszystkieMisje)
        {
            if (misja.objectives == null) continue;

            foreach (QuestObjective cel in misja.objectives)
            {
                if (tekstPrefab == null)
                {
                    Debug.LogError("[QuestLog] Brak przypisanego QuestTekst_Prefab w Inspektorze!");
                    continue;
                }

                // Spawnowanie linijki tekstu
                GameObject nowyTekst = Instantiate(tekstPrefab, contentParent);

                TextMeshProUGUI tmp = nowyTekst.GetComponent<TextMeshProUGUI>();
                if (tmp == null) tmp = nowyTekst.GetComponentInChildren<TextMeshProUGUI>();

                if (tmp != null)
                {
                    string status = cel.currentAmount >= cel.targetAmount ? "[X]" : "[ ]";
                    tmp.text = $"{status} {misja.questName} - {cel.objectiveDescription}: {cel.currentAmount}/{cel.targetAmount} ({misja.rewardGold}G)";

                    if (cel.currentAmount >= cel.targetAmount)
                    {
                        tmp.color = Color.green;
                        tmp.text = $"<s>{tmp.text}</s>";
                    }
                    else
                    {
                        tmp.color = Color.white;
                    }
                }
            }
        }
    }
}