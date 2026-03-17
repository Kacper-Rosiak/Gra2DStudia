using DungeonCore.Map.Model;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonCore.Map.Model
{
    /// <summary>
    /// Reprezentuje zhermetyzowany, pojedynczy wêze³ logiczny w z³o¿onym grafie lochu.
    /// Obiekt jest ca³kowicie wyizolowany z pêtli ¿ycia silnika Unity, gwarantuj¹c wydajnoœæ POCO.
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Bezwzglêdne koordynaty pokoju w dyskretnej, dwuwymiarowej przestrzeni kartezjañskiej Z^2.
        /// Wykorzystanie Vector2Int chroni przed problemami z b³êdami zmiennoprzecinkowymi (Floating Point Inaccuracies).
        /// </summary>
        public Vector2Int Position { get; private set; }

        /// <summary>
        /// Typologiczna, narracyjna rola pokoju dyktuj¹ca jego przysz³¹ zawartoœæ 
        /// mechaniczn¹ na poziomie silnika gry.
        /// </summary>
        public RoomType Type { get; private set; }

        /// <summary>
        /// S³ownik struktur generycznych przechowuj¹cy krawêdzie grafu (s¹siedztwa i drzwi). 
        /// Kluczem wyszukiwania jest wyliczenie kierunku, wartoœci¹ - bezpoœrednia referencja w pamiêci.
        /// Gwarantuje niezrównan¹ amortyzowan¹ z³o¿onoœæ wyszukiwania O(1).
        /// </summary>
        public Dictionary<Direction, Room> Neighbors { get; private set; }

        /// <summary>
        /// Konstruktor alokuj¹cy i inicjalizuj¹cy fundamentalny stan pokoju na podstawie 
        /// wstrzykniêtych parametrów koordynacyjnych.
        /// </summary>
        public Room(Vector2Int position, RoomType type)
        {
            Position = position;
            Type = type;
            // Alokacja instancji s³ownika na stercie w momencie tworzenia wêz³a
            Neighbors = new Dictionary<Direction, Room>();
        }

        /// <summary>
        /// Definiuje i rejestruje krawêdŸ pomiêdzy wierzcho³kami w obrêbie danego wêz³a.
        /// </summary>
        /// <param name="direction">Znormalizowany wektor wyjœciowy (miejsce wstawienia drzwi).</param>
        /// <param name="room">Referencja wskaŸnikowa na pokój docelowy.</param>
        public void AddNeighbor(Direction direction, Room room)
        {
            // Ochrona integralnoœci kolekcji przed prób¹ nadpisania przypisanej krawêdzi
            // lub dodania zduplikowanego klucza, co spowodowa³oby wyj¹tek czasowy (Runtime Exception).
            if (!Neighbors.ContainsKey(direction))
            {
                Neighbors.Add(direction, room);
            }
        }
    }
}