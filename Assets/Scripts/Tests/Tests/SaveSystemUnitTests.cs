using NUnit.Framework;
using UnityEngine;
using System;

// ============================================================================
// SEKCKJA 1: KOMPLETNY ZESTAW TESTÓW SYSTEMU ZAPISU (NUnit + EditMode)
// ============================================================================
[TestFixture]
public class SaveSystemUnitTests
{
    private PureSaveManager _saveManager;
    private MockStorage _mockStorage;

    [SetUp]
    public void Setup()
    {
        // Przed ka¿dym testem tworzymy czysty, wirtualny dysk twardy (Mock)
        _mockStorage = new MockStorage();
        _saveManager = new PureSaveManager(_mockStorage);
    }

    // --- TEST 1: BEZSTRATNY CYKL ZAPIS-ODCZYT ---

    [Test]
    public void SaveAndLoad_ValidData_IsLosslessAndIdentical()
    {
        // Given (Przygotowujemy stan gry)
        PureSaveData originalData = new PureSaveData { HP = 42, Gold = 100 };

        // When (Zapisujemy do wirtualnego pliku i natychmiast z niego wczytujemy)
        _saveManager.Save(originalData);
        PureSaveData loadedData = _saveManager.Load();

        // Then (Sprawdzamy, czy dane JSON po rozkodowaniu s¹ idealnie takie same)
        Assert.IsNotNull(loadedData, "B£¥D: Wczytane dane s¹ nullem!");
        Assert.AreEqual(42, loadedData.HP, "B£¥D: Utrata danych HP podczas deserializacji!");
        Assert.AreEqual(100, loadedData.Gold, "B£¥D: Utrata danych Z³ota podczas deserializacji!");
    }

    // --- TEST 2: BRAK PLIKU (CZYSTY PROFIL) ---

    [Test]
    public void Load_NoSaveFileExists_ReturnsCleanDefaultProfile()
    {
        // Given (Upewniamy siê, ¿e wirtualny plik nie istnieje)
        _mockStorage.FileExists = false;

        // When
        PureSaveData loadedData = _saveManager.Load();

        // Then (System nie mo¿e wywaliæ crasha, musi wydaæ czysty profil)
        Assert.IsNotNull(loadedData, "B£¥D: Brak pliku wywo³a³ b³¹d i zwróci³ null zamiast nowego profilu!");
        Assert.AreEqual(100, loadedData.HP, "B£¥D: Nowy profil powinien mieæ domyœlne 100 HP.");
        Assert.AreEqual(0, loadedData.Gold, "B£¥D: Nowy profil powinien mieæ domyœlne 0 Z³ota.");
    }

    // --- TEST 3: USZKODZONY PLIK (CORRUPTION) ---

    [Test]
    public void Load_CorruptedJsonData_HandlesGracefullyAndReturnsFallback()
    {
        // Given (Wrzucamy na wirtualny dysk totalny œmietnik, który nie jest formatem JSON)
        _mockStorage.FileExists = true;
        _mockStorage.Data = "TO_JEST_USZKODZONY_PLIK_{{[ERROR]";

        // When
        PureSaveData loadedData = _saveManager.Load();

        // Then (System musi to z³apaæ i nie pozwoliæ na wysypanie siê ca³ej gry)
        Assert.IsNotNull(loadedData, "B£¥D: Uszkodzony plik zniszczy³ system. Powinien zwróciæ czysty profil ratunkowy.");
        Assert.AreEqual(100, loadedData.HP, "B£¥D: Profil ratunkowy nie ustawi³ poprawnego HP.");
    }

    // --- TEST 4: WERSJONOWANIE (STARY ZAPIS) ---

    [Test]
    public void Load_OlderVersionSave_MigratesToCurrentVersion()
    {
        // Given (Symulujemy zapis z wersji gry v.1, obecna to v.2)
        // W starszej wersji gracz mia³ 50 HP.
        string oldVersionJson = "{\"Version\":1,\"HP\":50,\"Gold\":20}";
        _mockStorage.FileExists = true;
        _mockStorage.Data = oldVersionJson;

        // When
        PureSaveData loadedData = _saveManager.Load();

        // Then (System sam powinien wykryæ stary format, wczytaæ go i podbiæ mu wersjê)
        Assert.AreEqual(PureSaveManager.CURRENT_VERSION, loadedData.Version, "B£¥D: System nie zaktualizowa³ wersji pliku zapisu!");
        Assert.AreEqual(50, loadedData.HP, "B£¥D: Migracja wersji usunê³a postêpy gracza!");
        Assert.AreEqual(20, loadedData.Gold, "B£¥D: Migracja wersji usunê³a z³oto gracza!");
    }
}

// ============================================================================
// SEKCKJA 2: IZOLOWANA LOGIKA ZAPISU I WIRTUALNY DYSK (Wymóg 5.0)
// ============================================================================

// 1. Zwyk³a klasa trzymaj¹ca dane. U¿ywamy atrybutu Serializable dla konwertera JSON.
[Serializable]
public class PureSaveData
{
    public int Version;
    public int HP = 100; // Wartoœæ domyœlna
    public int Gold = 0; // Wartoœæ domyœlna
}

// 2. Interfejs dysku. Klucz do izolacji! 
// Dziêki temu nasz SaveManager nie obchodzi, czy to jest Windows, Android, czy nasz testowy RAM.
public interface IPureStorageProvider
{
    bool Exists();
    string Read();
    void Write(string jsonData);
}

// 3. ATRAPA DYSKU (Tylko na potrzeby testów EditMode)
public class MockStorage : IPureStorageProvider
{
    public bool FileExists = false;
    public string Data = "";

    public bool Exists() => FileExists;
    public string Read() => Data;
    public void Write(string jsonData)
    {
        Data = jsonData;
        FileExists = true;
    }
}

// 4. Mened¿er Zapisów. Zajmuje siê logik¹, JSON-em i b³êdami, a nie zapisem fizycznym.
public class PureSaveManager
{
    public const int CURRENT_VERSION = 2; // Obecna wersja gry to np. 2
    private IPureStorageProvider _storage;

    public PureSaveManager(IPureStorageProvider storage)
    {
        _storage = storage;
    }

    public void Save(PureSaveData data)
    {
        data.Version = CURRENT_VERSION;
        // U¿ywamy wbudowanego w Unity konwertera w trybie EditMode
        string json = JsonUtility.ToJson(data);
        _storage.Write(json);
    }

    public PureSaveData Load()
    {
        // Scenariusz: Brak pliku
        if (!_storage.Exists())
        {
            return new PureSaveData { Version = CURRENT_VERSION };
        }

        string json = _storage.Read();

        try
        {
            // Próba deserializacji JSON-a
            PureSaveData data = JsonUtility.FromJson<PureSaveData>(json);

            // Scenariusz: Uszkodzony lub pusty JSON, z którego wyszed³ null
            if (data == null || json.IndexOf('{') < 0)
            {
                return new PureSaveData { Version = CURRENT_VERSION };
            }

            // Scenariusz: Stara wersja zapisu (Migracja)
            if (data.Version < CURRENT_VERSION)
            {
                // Tutaj gra³aby logika przepisania starych statystyk do nowego systemu
                // Na razie po prostu podbijamy wersjê, ¿eby zapis by³ "aktualny" przy nastêpnym Save
                data.Version = CURRENT_VERSION;
            }

            return data;
        }
        catch (Exception)
        {
            // Scenariusz: Krytyczne uszkodzenie sk³adni JSON (Crash Prevention)
            // System po cichu oddaje czysty profil, nie blokuj¹c wejœcia do menu.
            return new PureSaveData { Version = CURRENT_VERSION };
        }
    }
}