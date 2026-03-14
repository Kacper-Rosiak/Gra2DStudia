using DungeonCore.Map.Model;
using System;

namespace DungeonCore.Map.Model
{
    /// <summary>
    /// Integralna klasa œrodowiskowa steruj¹ca bezb³êdnie logik¹ liniowego i nieliniowego 
    /// poruszania siê po wykreowanym matematycznie grafie abstrakcyjnej struktury podziemi.
    /// Pe³ni zaszczytn¹ funkcjê zwinnego Agenta Kontrolera, zarz¹dzaj¹c wy³¹cznym indeksem pokoju.
    /// </summary>
    public class DungeonManager
    {
        /// <summary>
        /// Hermetyczne, wewnêtrzne odniesienie do gigantycznej przestrzennej struktury ca³ego lochu (Model Domenowy).
        /// Stanowi jedyne zrod³o prawdy (Single Source of Truth) dla weryfikatora kolizji topologicznej.
        /// </summary>
        public DungeonGraph Graph { get; private set; }

        /// <summary>
        /// Krytyczny wskaŸnik alokacji pamiêciowej (Index), w czasie rzeczywistym nieub³aganie 
        /// œledz¹cy i weryfikuj¹cy referencjê do pokoju bazowego, w którym znajduje siê w tej sekundzie gry
        /// byt przestrzenny taki jak podmiot gracza lub g³ówny bohater narracyjny.
        /// Zmiana odniesienia w tym polu w sposób doskona³y symuluje abstrakcyjne i kosztowne obliczeniowo "przejœcie".
        /// </summary>
        public Room CurrentRoom { get; private set; }

        /// <summary>
        /// C# native event (Zdarzenie deleguj¹ce oparte o silnie typowane Action). 
        /// Konstrukcja wybitnie przydatna i wrêcz niezbêdna do ca³kowicie bezproblemowej 
        /// integracji z asynchronicznym, docelowym wielow¹tkowym œrodowiskiem UI 
        /// w silniku Unity. Stanowi pasywny wektor powiadamiaj¹cy zewnêtrzny modu³ widoku
        /// o nowej dystrybucji elementów œrodowiskowych bez twardego sprzêgania architektury (loose coupling).
        /// </summary>
        public event Action<Room> OnRoomChanged;

        /// <summary>
        /// Wywo³anie konstruktora bezpiecznie inicjalizuje hermetyczn¹ logikê poprzez bezkompromisowe wykreowanie 
        /// ca³kowicie suwerennej instancji skomplikowanego grafu wêz³owego mapy oraz narzucenie
        /// domyœlnego wektora pozycji obiektu gracza dok³adnie na progu wygenerowanego œrodowiska (StartRoom).
        /// </summary>
        public DungeonManager()
        {
            // Natychmiastowe utworzenie strukturalne przy tworzeniu mened¿era
            Graph = new DungeonGraph();

            // Koniecznoœæ buforowania i tak zwanego "cache'owania pierwszego punktu styku". 
            // Znalezienie koordynat w matrycach o rozmiarze milionów jednostek kwadratowych  
            // bez tej logiki po¿ar³oby ogromny odsetek cykli na poszukiwanie (Search Time).
            CurrentRoom = Graph.StartRoom;

            // Niespodziewana dereferencja wskaŸnikowa mog³aby zawiesiæ logikê systemow¹.
            if (CurrentRoom != null)
            {
                // Inicjalizacyjne opublikowanie globalnego stanu œrodowiskowego na potrzeby testów
                OnRoomChanged?.Invoke(CurrentRoom);
            }
            else
            {
                // Ekstremalne b³êdy na warstwie grafowej winny generowaæ hard-crash przy testach jednostkowych.
                throw new InvalidOperationException("Inicjalizacja œrodowiska zaniechana. Graf lochu nie posiada prawid³owo alokowanego punktu startowego StartRoom.");
            }
        }

        /// <summary>
        /// Rdzeñ mechaniki testowej podmiotu - wysoce zoptymalizowana wirtualna metoda ruchu. 
        /// Bezkompromisowo weryfikuje oraz (wystêpuj¹c w warunkach absolutnej pewnoœci logicznej)
        /// pomyœlnie wykonuje nag³¹ cyfrow¹ tranzycjê w rygorystycznie zadanym przez strumieñ wejœcia
        /// kierunku wektorowym opartym na ortogonalnych osiach odniesienia kartezjañskiego.
        /// Ca³kowicie izoluje z³o¿onoœæ warstwy danych, zrêcznie ukrywaj¹c proces sprawdzania kolekcji grafu.
        /// </summary>
        /// <param name="dir">Skompresowany do wartoœci wyliczeniowej kierunek, na wektorze którego agent zlecaj¹cy dokonuje próby tranzycji geometrycznej.</param>
        /// <returns>Logiczna flaga potwierdzaj¹ca stan - Prawda (true), gwarantuj¹ca pomyœln¹ synchronizacjê przejœcia wskaŸnikowego, w ka¿dym innym defektywnym wypadku - Fa³sz (false).</returns>
        public bool MoveTo(Direction dir)
        {
            // Operacja siêga g³êboko do wnêtrznoœci wyizolowanego wêz³a i wy³uskuje dane przy u¿yciu 
            // potê¿nego wariantu czasowego wyszukiwania s³ownikowego TryGetValue o skali absolutnej O(1).
            // Niszczy to w zarodku b³êdy nadmiernego iterowania matryc wielowymiarowych uci¹¿liwych w starych paradygmatach.
            if (CurrentRoom.Neighbors.TryGetValue(dir, out Room destinationRoom))
            {
                // Fizyczna podmiana wyabstrahowanego z przestrzeni 3D wskaŸnika (przypisanie nowego indeksu wêz³a referencyjnego)
                CurrentRoom = destinationRoom;

                // Bezzw³oczna, pozbawiona cykli zatrzymuj¹cych (blocking cycles) powszechna emisja zdarzenia powiadamiaj¹cego
                // warstwê dewelopersk¹ zintegrowan¹ lub ostateczny system warstwy widoku w silniku 
                // o diametralnej reorientacji geometrii wokó³ agenta, zlecaj¹c zmianê renderingu lub logowania zdarzeñ.
                OnRoomChanged?.Invoke(CurrentRoom);

                return true; // Stan symulacji zaktualizowany i zweryfikowany z ca³kowitym sukcesem
            }

            // Osi¹gniêcie tej strefy powrotnej implikuje krytyczny brak definicji krawêdzi nawigacyjnej, zjawisko uderzenia w pustkê lub œcianê absolutn¹. 
            // Odmowa autoryzacji przesuniêcia wektora zabezpiecza ca³¹ fizykê silnika przed wyjœciem za ramy tablicy indeksów.
            // Bezpieczne zatrzymanie bez u¿ycia wyj¹tku.
            return false;
        }
    }
}