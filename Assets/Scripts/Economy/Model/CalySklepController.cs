using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CalySklepController : MonoBehaviour
{
    [Header("Ekwipunek Gracza")]
    // Tutaj wrzucisz pliki ItemData z folderu projektu w Unity, żeby sklep wiedział co daje graczowi!
    public List<ItemData> przedmiotyWojownika = new List<ItemData>();
    public List<ItemData> przedmiotyMaga = new List<ItemData>();

    [Header("Sklep Wojownika")]
    public GameObject oknoWojownika;
    public TextMeshProUGUI goldTextWojownik;
    public Button przyciskMlot;
    public Button przyciskSztylet;
    public Button przyciskLuk;
    public Button przyciskTarcza;

    [Header("Sklep Maga")]
    public GameObject oknoMaga;
    public TextMeshProUGUI goldTextMag;
    public Button przyciskMikstura;
    public Button przyciskKostur;
    public Button przyciskKula;
    public Button przyciskWidly;

    private bool _isPlayerNearby = false;
    private string _klasaGraczaCache = "Warrior";

    // Referencja do ekwipunku, który aktualnie ma gracz
    private Inventory _inventoryGracza;

    private void Start()
    {
        if (oknoWojownika != null) oknoWojownika.SetActive(false);
        if (oknoMaga != null) oknoMaga.SetActive(false);

        // SZUKANIE EKWIPUNKU: Szukamy w grze komponentu, który trzyma Twój obiekt 'Inventory'
        // Najczęściej wisi on na obiekcie z Playerem albo GameManagerem.
        // Zakładam, że masz skrypt typu PlayerController lub InventoryUI, który tworzy "new Inventory()".
        // Dla testu pobierzemy go z managera lub gracza, ale na ten moment, żeby nie wywaliło błędu,
        // stworzymy instancję testową, jeśli gra jej nie znajdzie automatycznie:
        _inventoryGracza = new Inventory(500); // Startowo 500 złota w Twoim systemie Inventory!

        // Podpięcie przycisków Wojownika (indeksy 0, 1, 2, 3 z listy)
        if (przyciskMlot != null) { przyciskMlot.onClick.RemoveAllListeners(); przyciskMlot.onClick.AddListener(() => ProbaZakupu(0, true)); }
        if (przyciskSztylet != null) { przyciskSztylet.onClick.RemoveAllListeners(); przyciskSztylet.onClick.AddListener(() => ProbaZakupu(1, true)); }
        if (przyciskLuk != null) { przyciskLuk.onClick.RemoveAllListeners(); przyciskLuk.onClick.AddListener(() => ProbaZakupu(2, true)); }
        if (przyciskTarcza != null) { przyciskTarcza.onClick.RemoveAllListeners(); przyciskTarcza.onClick.AddListener(() => ProbaZakupu(3, true)); }

        // Podpięcie przycisków Maga (indeksy 0, 1, 2, 3 z listy)
        if (przyciskMikstura != null) { przyciskMikstura.onClick.RemoveAllListeners(); przyciskMikstura.onClick.AddListener(() => ProbaZakupu(0, false)); }
        if (przyciskKostur != null) { przyciskKostur.onClick.RemoveAllListeners(); przyciskKostur.onClick.AddListener(() => ProbaZakupu(1, false)); }
        if (przyciskKula != null) { przyciskKula.onClick.RemoveAllListeners(); przyciskKula.onClick.AddListener(() => ProbaZakupu(2, false)); }
        if (przyciskWidly != null) { przyciskWidly.onClick.RemoveAllListeners(); przyciskWidly.onClick.AddListener(() => ProbaZakupu(3, false)); }

        AktualizujZlotoUI();
    }

    private void Update()
    {
        if (_isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if ((oknoWojownika != null && oknoWojownika.activeSelf) || (oknoMaga != null && oknoMaga.activeSelf))
            {
                ZamknijSklepy();
            }
            else
            {
                OtworzSklepDlaKlasy();
            }
        }
    }

    private void OtworzSklepDlaKlasy()
    {
        AktualizujZlotoUI();
        if (_klasaGraczaCache == "Warrior" && oknoWojownika != null) oknoWojownika.SetActive(true);
        if (_klasaGraczaCache == "Mage" && oknoMaga != null) oknoMaga.SetActive(true);
    }

    private void ZamknijSklepy()
    {
        if (oknoWojownika != null) oknoWojownika.SetActive(false);
        if (oknoMaga != null) oknoMaga.SetActive(false);
    }

    private void ProbaZakupu(int indeksPrzedmiotu, bool czyWojownik)
    {
        List<ItemData> listaDoKupna = czyWojownik ? przedmiotyWojownika : przedmiotyMaga;

        // Bezpiecznik: Czy w ogóle wrzuciłeś ten przedmiot do listy w Inspektorze?
        if (indeksPrzedmiotu >= listaDoKupna.Count || listaDoKupna[indeksPrzedmiotu] == null)
        {
            Debug.LogError($"[SKLEP] Brak przypisanego pliku ItemData dla slotu {indeksPrzedmiotu}!");
            return;
        }

        ItemData wybranyPrzedmiot = listaDoKupna[indeksPrzedmiotu];

        // Zakładamy sztywne ceny tak jak były na tablicach: 50, 60, 80, 95
        int[] ceny = { 50, 60, 80, 95 };
        int cena = ceny[Mathf.Clamp(indeksPrzedmiotu, 0, 3)];

        // UŻYWAMY TWOJEJ FUNKCJI Z INVENTORY: TrySpendGold sama sprawdzi kasę i ją odejmie!
        if (_inventoryGracza != null && _inventoryGracza.TrySpendGold(cena))
        {
            // UŻYWAMY TWOJEJ FUNKCJI Z INVENTORY: Dodajemy fizyczny obiekt do plecaka!
            _inventoryGracza.AddItem(wybranyPrzedmiot);

            AktualizujZlotoUI();
            Debug.Log($"[SKLEP DODANO!] Kupiono: {wybranyPrzedmiot.name} i dodano do Twojego Inventory!");
        }
        else
        {
            Debug.LogError($"[SKLEP] Za mało kasy w Twoim Inventory! Koszt: {cena}, masz: {_inventoryGracza?.Gold}");
        }
    }

    private void AktualizujZlotoUI()
    {
        if (_inventoryGracza == null) return;

        // Pobieramy złoto bezpośrednio z Twojego systemu Inventory!
        if (goldTextWojownik != null) goldTextWojownik.text = _inventoryGracza.Gold + "G";
        if (goldTextMag != null) goldTextMag.text = _inventoryGracza.Gold + "G";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerNearby = true;

            // Tutaj możesz zmienić na "Mage" żeby sprawdzić sklep maga
            _klasaGraczaCache = "Warrior";
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerNearby = false;
            ZamknijSklepy();
        }
    }
}