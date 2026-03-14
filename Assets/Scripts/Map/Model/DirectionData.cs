using System.Collections.Generic;
using UnityEngine; // Wykorzystywane WY£¥CZNIE dla wysoce zoptymalizowanej struktury wartoœciowej Vector2Int

namespace DungeonCore.Map.Model
{
    /// <summary>
    /// Klasyfikacja kierunków ortogonalnych operuj¹cych w przestrzeni siatki 2D.
    /// Typ bazowy int gwarantuje b³yskawiczne operacje bitowe.
    /// </summary>
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    /// <summary>
    /// Semantyczna, wysokopoziomowa klasyfikacja ról poszczególnych wêz³ów 
    /// w pe³ni zgodna z architektur¹ projektowania poziomów "5-Room Dungeon".
    /// </summary>
    public enum RoomType
    {
        Entrance,
        Puzzle,
        Setback,
        Boss,
        Reward,
        Generic // Typ awaryjny dla masywnych algorytmów proceduralnych
    }

    /// <summary>
    /// Biblioteka rozszerzeñ wspomagaj¹cych skomplikowane operacje na kierunkach.
    /// Zapewnia rygorystyczn¹ logikê topologiczn¹ dla odwracania wektorów po³¹czeñ grafu.
    /// </summary>
    public static class DirectionExtensions
    {
        /// <summary>
        /// Oblicza i zwraca kierunek przeciwstawny w osiach ortogonalnych kartezjañskich.
        /// Procedura niezbêdna do poprawnego wi¹zania dwukierunkowych krawêdzi nieskierowanych.
        /// </summary>
        public static Direction GetOpposite(this Direction dir)
        {
            return dir switch
            {
                Direction.North => Direction.South,
                Direction.East => Direction.West,
                Direction.South => Direction.North,
                Direction.West => Direction.East,
                // Ochrona przed b³êdn¹ rzutowaniem typów wyliczeniowych w pamiêci
                _ => throw new System.ArgumentOutOfRangeException(nameof(dir), $"Nierozpoznany wektor kierunkowy: {dir}")
            };
        }

        /// <summary>
        /// Dokonuje translacji enumeratora kierunkowego na matematyczny wektor przestrzenny 2D.
        /// Metoda pozwala na b³yskawiczn¹ kalkulacjê absolutnych wspó³rzêdnych kolejnego pokoju
        /// w procedurach generowania strukturalnego mapy.
        /// </summary>
        public static Vector2Int ToVector2Int(this Direction dir)
        {
            return dir switch
            {
                Direction.North => new Vector2Int(0, 1),
                Direction.East => new Vector2Int(1, 0),
                Direction.South => new Vector2Int(0, -1),
                Direction.West => new Vector2Int(-1, 0),
                _ => Vector2Int.zero
            };
        }
    }
}