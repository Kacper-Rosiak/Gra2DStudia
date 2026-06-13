using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalySklepController : MonoBehaviour
{
    [Header("Sklep Wojownika")]
    public GameObject oknoWojownika;
    public TextMeshProUGUI goldTextWojownik;
    public Button przyciskMlot;
    public Button przyciskSztylet;
    public Button przyciskLuk;
    public Button przyciskTarcza;

    [Header("Sklep Maga (Przyciski IDEALNIE od góry do dołu)")]
    public GameObject oknoMaga;
    public TextMeshProUGUI goldTextMag;
    public Button magPrzycisk_KosturKosnika; // 1 od góry
    public Button magPrzycisk_LaskaSekata;   // 2 od góry
    public Button magPrzycisk_KosturRunow;   // 3 od góry
    public Button magPrzycisk_EliksirZdrowia; // 4 od góry

    [Header("Bazy Przedmiotow")]
    public ItemData[] _bazaWojownika = new ItemData[4];
    public ItemData[] _bazaMaga = new ItemData[4];

    private bool _isPlayerNearby = false;
    private string _klasaGraczaCache = "Warrior";

    private void Start()
    {
        if (oknoWojownika != null) oknoWojownika.SetActive(false);
        if (oknoMaga != null) oknoMaga.SetActive(false);

        // PODPIĘCIE PRZYCISKÓW WOJOWNIKA
        if (przyciskMlot != null) { przyciskMlot.onClick.RemoveAllListeners(); przyciskMlot.onClick.AddListener(() => KupnoZListy(0, true, 60)); }
        if (przyciskSztylet != null) { przyciskSztylet.onClick.RemoveAllListeners(); przyciskSztylet.onClick.AddListener(() => KupnoZListy(1, true, 80)); }
        if (przyciskLuk != null) { przyciskLuk.onClick.RemoveAllListeners(); przyciskLuk.onClick.AddListener(() => KupnoZListy(2, true, 95)); }
        if (przyciskTarcza != null) { przyciskTarcza.onClick.RemoveAllListeners(); przyciskTarcza.onClick.AddListener(() => KupnoZListy(3, true, 50)); }

        // =========================================================================
        // IDEALNA KOLEJNOŚĆ DLA MAGA (SZTYWNE INDEKSY Z BAZY)
        // =========================================================================

        // 1. Kostur Kośnika -> Zawsze kupuje Element 0 z Bazy Maga, cena 60G
        if (magPrzycisk_KosturKosnika != null) { magPrzycisk_KosturKosnika.onClick.RemoveAllListeners(); magPrzycisk_KosturKosnika.onClick.AddListener(() => WykonajZakupMaga(0, 60)); }

        // 2. Laska Sękata -> Zawsze kupuje Element 1 z Bazy Maga, cena 80G
        if (magPrzycisk_LaskaSekata != null) { magPrzycisk_LaskaSekata.onClick.RemoveAllListeners(); magPrzycisk_LaskaSekata.onClick.AddListener(() => WykonajZakupMaga(1, 80)); }

        // 3. Kostur Runów -> Zawsze kupuje Element 2 z Bazy Maga, cena 95G
        if (magPrzycisk_KosturRunow != null) { magPrzycisk_KosturRunow.onClick.RemoveAllListeners(); magPrzycisk_KosturRunow.onClick.AddListener(() => WykonajZakupMaga(2, 95)); }

        // 4. Eliksir Zdrowia -> Zawsze kupuje Element 3 z Bazy Maga, cena 50G
        if (magPrzycisk_EliksirZdrowia != null) { magPrzycisk_EliksirZdrowia.onClick.RemoveAllListeners(); magPrzycisk_EliksirZdrowia.onClick.AddListener(() => WykonajZakupMaga(3, 50)); }

        AktualizujZlotoUI();
    }

    private void WykonajZakupMaga(int indeks, int cena)
    {
        if (_bazaMaga == null || indeks >= _bazaMaga.Length || _bazaMaga[indeks] == null)
        {
            Debug.LogError($"[SKLEP] Brak przypisanego przedmiotu w Baza Maga pod Elementem {indeks}!");
            return;
        }

        PlayerManager realPlayerManager = PlayerManager.Instance;
        if (realPlayerManager != null && realPlayerManager.Inventory != null)
        {
            if (realPlayerManager.Inventory.TrySpendGold(cena))
            {
                realPlayerManager.Inventory.AddItem(_bazaMaga[indeks]);
                AktualizujZlotoUI();
                Debug.Log($"[SKLEP SUKCES] Kupiono dla Maga: {_bazaMaga[indeks].name}. Zostało konta: {realPlayerManager.Inventory.Gold}G");

                // POWIADOMIENIE SYSTEMU MISJI
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.ZwiekszPostepCelu("Kup dowolny przedmiot u handlarza");
                }
            }
            else
            {
                Debug.LogError($"[SKLEP] Brak kasy na: {_bazaMaga[indeks].name}! Koszt: {cena}G");
            }
        }
        else
        {
            Debug.LogError("[SKLEP ERROR] Nie znaleziono skryptu plecaka gracza!");
        }
    }

    private void KupnoZListy(int indeks, bool czyWojownik, int cena)
    {
        if (_bazaWojownika == null || indeks >= _bazaWojownika.Length || _bazaWojownika[indeks] == null) return;

        PlayerManager realPlayerManager = PlayerManager.Instance;
        if (realPlayerManager != null && realPlayerManager.Inventory != null)
        {
            if (realPlayerManager.Inventory.TrySpendGold(cena))
            {
                realPlayerManager.Inventory.AddItem(_bazaWojownika[indeks]);
                AktualizujZlotoUI();
            }
        }
    }

    private void Update()
    {
        if (_isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if ((oknoWojownika != null && oknoWojownika.activeSelf) || (oknoMaga != null && oknoMaga.activeSelf))
            {
                if (oknoWojownika != null) oknoWojownika.SetActive(false);
                if (oknoMaga != null) oknoMaga.SetActive(false);
            }
            else
            {
                PlayerManager realPlayerManager = PlayerManager.Instance;
                if (realPlayerManager != null && realPlayerManager.startingClass != null)
                {
                    _klasaGraczaCache = realPlayerManager.startingClass.className;
                }
                else
                {
                    _klasaGraczaCache = "Mage";
                }

                AktualizujZlotoUI();

                if (_klasaGraczaCache == "Warrior" && oknoWojownika != null) oknoWojownika.SetActive(true);
                if (_klasaGraczaCache == "Mage" && oknoMaga != null) oknoMaga.SetActive(true);

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        if ((oknoWojownika != null && oknoWojownika.activeSelf) || (oknoMaga != null && oknoMaga.activeSelf))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void AktualizujZlotoUI()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.Inventory != null)
        {
            int zloto = PlayerManager.Instance.Inventory.Gold;
            if (goldTextWojownik != null) goldTextWojownik.text = zloto + "G";
            if (goldTextMag != null) goldTextMag.text = zloto + "G";
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) _isPlayerNearby = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) _isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerNearby = false;
            if (oknoWojownika != null) oknoWojownika.SetActive(false);
            if (oknoMaga != null) oknoMaga.SetActive(false);
        }
    }
}