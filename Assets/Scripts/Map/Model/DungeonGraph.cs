using DungeonCore.Map.Model;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonCore.Map.Model
{
    /// <summary>
    /// Rozleg³a struktura architektoniczna, w pe³ni odpowiedzialna za definicjê ca³ego grafu mapy.
    /// Nadzoruje spójnoœæ wskaŸnikow¹ i pamiêciow¹ wszystkich zagnie¿d¿onych w nim logicznie wêz³ów.
    /// </summary>
    public class DungeonGraph
    {
        // Prywatny, globalny rejestr wszystkich wêz³ów, mapuj¹cy wektor pozycji na konkretny pokój.
        // Tworzy zoptymalizowany indeks przestrzenny, diametralnie przyspieszaj¹c zapytania strukturalne O(1).
        private Dictionary<Vector2Int, Room> _roomRegistry;

        /// <summary>
        /// Niezmienny punkt centralny i wejœciowy - absolutny korzeñ grafu wêz³owego mapy.
        /// </summary>
        public Room StartRoom { get; private set; }

        public DungeonGraph()
        {
            _roomRegistry = new Dictionary<Vector2Int, Room>();
            BuildStaticTestMap();
        }

        /// <summary>
        /// Enkapsuluje z³o¿on¹ logikê gwarantuj¹c¹ matematycznie spójne i obustronne powi¹zanie 
        /// pomiêdzy dwoma dynamicznymi wêz³ami na rygorystycznie zadanym wektorze kierunku. 
        /// Operacja symetryczna: Jeœli wêze³ A spogl¹da na Pó³noc w stronê wêz³a B, 
        /// wektor B si³¹ narzuca spojrzenie na Po³udnie wzglêdem wêz³a A.
        /// </summary>
        private void ConnectRooms(Room roomA, Direction dirAtoB, Room roomB)
        {
            // Ochrona przed krytycznym b³êdem NullReferenceException psuj¹cym integralnoœæ silnika.
            if (roomA == null || roomB == null) return;

            // Inicjalizacja krawêdzi w wektorze natarcia...
            roomA.AddNeighbor(dirAtoB, roomB);

            //...i natychmiastowe zdefiniowanie wektora powrotnego za spraw¹ metod rozszerzaj¹cych.
            Direction oppositeDir = dirAtoB.GetOpposite();
            roomB.AddNeighbor(oppositeDir, roomA);
        }

        /// <summary>
        /// Konstruuje w pamiêci niezwykle rygorystyczn¹ logicznie, z góry przewidzian¹ mapê testow¹.
        /// Sk³ada siê ona z wysoce zoptymalizowanych, zdefiniowanych "na sztywno" koordynat.
        /// </summary>
        private void BuildStaticTestMap()
        {
            /* Deklaratywny schemat przestrzenny - Topologia "Krzy¿a/Litery T" (5-node cross subgraph):
             * Uk³ad ten unika b³êdu linearnoœci i stwarza pole do podejmowania wieloœcie¿kowych decyzji.
             *
             *                  (0, 2)
             * | (Pó³noc/Po³udnie)
             * [Puzzle ] (0, 0) -- (0, 1) -- (1, 1)  <-- (Wschód/Zachód)
             * | (Pó³noc/Po³udnie)
             *                 [Entrance] (0, -1)
             */

            // Faza 1: Instancjonowanie obiektów strukturalnych wêz³ów i nadawanie ról narracyjnych.
            Room entrance = new Room(new Vector2Int(0, -1), RoomType.Entrance);
            Room setback = new Room(new Vector2Int(0, 1), RoomType.Setback);
            Room puzzle = new Room(new Vector2Int(0, 0), RoomType.Puzzle);
            Room boss = new Room(new Vector2Int(0, 2), RoomType.Boss);
            Room reward = new Room(new Vector2Int(1, 1), RoomType.Reward);

            // Faza 2: Dynamiczna rejestracja wêz³ów w super-wydajnym indeksie przestrzennym.
            _roomRegistry.Add(entrance.Position, entrance);
            _roomRegistry.Add(puzzle.Position, puzzle);
            _roomRegistry.Add(setback.Position, setback);
            _roomRegistry.Add(boss.Position, boss);
            _roomRegistry.Add(reward.Position, reward);

            // Faza 3: Fizyczne "kucie w kamieniu" nieodwracalnych krawêdzi uk³adu przestrzennego.
            // Oparte na bezpiecznej metodzie automatycznie dbaj¹cej o wektory odwrotne.
            ConnectRooms(entrance, Direction.North, puzzle);
            ConnectRooms(puzzle, Direction.North, setback);
            ConnectRooms(setback, Direction.North, boss);
            ConnectRooms(setback, Direction.East, reward);

            // Wyj¹tkowo kluczowe w dewelopmencie: Zapisanie wskaŸnika korzenia przestrzennego,
            // zapobiegaj¹c koniecznoœci ¿mudnego wyszukiwania go przy ponownej inicjalizacji gracza.
            StartRoom = entrance;
        }

        /// <summary>
        /// Umo¿liwia zewnêtrznym, asynchronicznym systemom (jak np. algorytmy AI nawigacji)
        /// b³yskawiczne pozyskanie bezpoœredniej referencji do pokoju na podanych koordynatach.
        /// </summary>
        public Room GetRoomAt(Vector2Int position)
        {
            // U¿ycie paradygmatu TryGetValue ca³kowicie eliminuje nadmiern¹ alokacjê
            // zg³aszania wyj¹tków w przypadku odpytywania pustych wspó³rzêdnych "czarnej przestrzeni".
            if (_roomRegistry.TryGetValue(position, out Room room))
            {
                return room;
            }
            return null;
        }
    }
}