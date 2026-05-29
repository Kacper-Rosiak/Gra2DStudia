using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

// ============================================================================
// SEKCKJA 1: KOMPLETNY ZESTAW TESTÓW GENERACJI MAPY (NUnit + EditMode)
// ============================================================================
[TestFixture]
public class MapGenerationUnitTests
{
    private PureMapGenerator _generator;

    [SetUp]
    public void Setup()
    {
        // Konfiguracja: Siatka 100x100, liczba pokoi od 5 do 10
        _generator = new PureMapGenerator(gridWidth: 100, gridHeight: 100, minRooms: 5, maxRooms: 10);
    }

    // --- TEST ILOŒCI ---

    [Test]
    public void Generate_RoomsCount_IsWithinSpecifiedBounds()
    {
        // Kiedy wykonujemy algorytm kilkukrotnie, aby sprawdziæ losowoœæ
        for (int i = 0; i < 10; i++)
        {
            List<PureRoom> rooms = _generator.GenerateMap();

            // Wtedy upewniamy siê, ¿e liczba komnat zawsze mieœci siê w wide³kach
            Assert.IsTrue(rooms.Count >= 5, $"B£¥D: Wygenerowano za ma³o pokoi ({rooms.Count} < 5)");
            Assert.IsTrue(rooms.Count <= 10, $"B£¥D: Wygenerowano za du¿o pokoi ({rooms.Count} > 10)");
        }
    }

    // --- TEST GRANIC (BOUNDARIES) ---

    [Test]
    public void Generate_RoomsPosition_DoesNotExceedGridBoundaries()
    {
        // Given
        int expectedMaxWidth = 100;
        int expectedMaxHeight = 100;

        // When
        List<PureRoom> rooms = _generator.GenerateMap();

        // Then
        foreach (var room in rooms)
        {
            bool isXValid = room.X >= 0 && (room.X + room.Width) <= expectedMaxWidth;
            bool isYValid = room.Y >= 0 && (room.Y + room.Height) <= expectedMaxHeight;

            Assert.IsTrue(isXValid, $"B£¥D: Pokój wychodzi poza mapê na osi X! (X: {room.X}, Szerokoœæ: {room.Width})");
            Assert.IsTrue(isYValid, $"B£¥D: Pokój wychodzi poza mapê na osi Y! (Y: {room.Y}, Wysokoœæ: {room.Height})");
        }
    }

    // --- TEST SPÓJNOŒCI (CONNECTIVITY) ---

    [Test]
    public void Generate_AllRooms_AreConnectedToEachOther()
    {
        // When
        List<PureRoom> rooms = _generator.GenerateMap();

        // Then (Sprawdzamy za pomoc¹ prostego algorytmu przeszukiwania grafu / Flood Fill)
        Assert.IsTrue(rooms.Count > 0, "B£¥D: Mapa jest pusta!");

        List<PureRoom> connectedRooms = new List<PureRoom>();
        Queue<PureRoom> roomsToCheck = new Queue<PureRoom>();

        // Zaczynamy od pokoju startowego (indeks 0)
        roomsToCheck.Enqueue(rooms[0]);
        connectedRooms.Add(rooms[0]);

        while (roomsToCheck.Count > 0)
        {
            PureRoom current = roomsToCheck.Dequeue();

            // Szukamy s¹siadów, z którymi obecny pokój siê przecina/styka
            foreach (var otherRoom in rooms)
            {
                if (!connectedRooms.Contains(otherRoom) && current.IntersectsOrTouches(otherRoom))
                {
                    connectedRooms.Add(otherRoom);
                    roomsToCheck.Enqueue(otherRoom);
                }
            }
        }

        // Jeœli algorytm znalaz³ przejœcie do wszystkich pokoi, to zbiory powinny byæ równe
        Assert.AreEqual(rooms.Count, connectedRooms.Count, "B£¥D: Znaleziono 'p³ywaj¹ce' (niepo³¹czone) komnaty na mapie!");
    }
}

// ============================================================================
// SEKCKJA 2: IZOLOWANA LOGIKA GENERATORA MAPY (Zgodnoœæ z architektur¹ 5.0)
// ============================================================================

public class PureRoom
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public PureRoom(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    // Funkcja sprawdzaj¹ca czy dwa pokoje siê stykaj¹ lub nachodz¹ na siebie (AABB)
    public bool IntersectsOrTouches(PureRoom other)
    {
        return (this.X <= other.X + other.Width &&
                this.X + this.Width >= other.X &&
                this.Y <= other.Y + other.Height &&
                this.Y + this.Height >= other.Y);
    }
}

public class PureMapGenerator
{
    private int _gridWidth;
    private int _gridHeight;
    private int _minRooms;
    private int _maxRooms;
    private Random _rng;

    public PureMapGenerator(int gridWidth, int gridHeight, int minRooms, int maxRooms)
    {
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        _minRooms = minRooms;
        _maxRooms = maxRooms;
        _rng = new Random();
    }

    public List<PureRoom> GenerateMap()
    {
        List<PureRoom> rooms = new List<PureRoom>();
        int roomCount = _rng.Next(_minRooms, _maxRooms + 1);

        for (int i = 0; i < roomCount; i++)
        {
            // Losujemy rozmiar komnaty
            int width = _rng.Next(5, 15);
            int height = _rng.Next(5, 15);

            int x, y;

            if (rooms.Count == 0)
            {
                // Pierwszy pokój zawsze l¹duje losowo na siatce (ale nie wychodzi poza ni¹)
                x = _rng.Next(0, _gridWidth - width + 1);
                y = _rng.Next(0, _gridHeight - height + 1);
            }
            else
            {
                // Aby zagwarantowaæ SPÓJNOŒÆ (wymóg 5.0), ka¿d¹ kolejn¹ komnatê
                // "przyklejamy" do którejœ z ju¿ istniej¹cych komnat
                PureRoom attachedTo = rooms[_rng.Next(0, rooms.Count)];

                // Losujemy pozycjê styku z wybranym pokojem
                x = _rng.Next(Math.Max(0, attachedTo.X - width + 1), Math.Min(_gridWidth - width, attachedTo.X + attachedTo.Width - 1));
                y = attachedTo.Y + attachedTo.Height; // Uproszczone: przyklejamy do górnej œciany

                // Jeœli uciek³oby to poza siatkê Y, wpychamy do œrodka (wymóg GRANIC)
                if (y + height > _gridHeight)
                {
                    y = _gridHeight - height;
                    x = attachedTo.X; // Korygujemy, ¿eby wci¹¿ siê styka³y
                }
            }

            rooms.Add(new PureRoom(x, y, width, height));
        }

        return rooms;
    }
}