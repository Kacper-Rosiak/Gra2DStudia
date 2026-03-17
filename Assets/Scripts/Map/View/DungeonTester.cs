using UnityEngine;
using UnityEngine.InputSystem;
using DungeonCore.Map.Model; // Pobiera informacje z naszej warstwy danych

namespace DungeonCore.Map.View
{
    public class DungeonTester : MonoBehaviour
    {
        private DungeonManager _dungeonManager;

        void Start()
        {
            // Inicjalizuje mened¿era, co powoduje wygenerowanie grafu podziemi.
            // Buforowanie pierwszego pokoju na samym pocz¹tku u³atwia póŸniejsze umieszczenie gracza na mapie.
            _dungeonManager = new DungeonManager();

            // Podpiêcie pod zdarzenie: za ka¿dym razem, gdy zmieni siê pokój, wypisz to w konsoli.
            _dungeonManager.OnRoomChanged += (room) =>
            {
                Debug.Log($"Gracz wszed³ do pokoju typu: {room.Type} na wspó³rzêdnych {room.Position}");
            };
        }

        void Update()
        {
            // Zabezpieczenie przed brakiem klawiatury
            if (Keyboard.current == null) return;

            // Nas³uchiwanie wciœniêcia strza³ek z u¿yciem nowego systemu
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                TryMove(Direction.North, "Pó³noc");
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                TryMove(Direction.South, "Po³udnie");
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                TryMove(Direction.East, "Wschód");
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                TryMove(Direction.West, "Zachód");
            }
        }

        // Metoda pomocnicza wykonuj¹ca ruch i sprawdzaj¹ca, czy uderzyliœmy w œcianê
        private void TryMove(Direction direction, string directionName)
        {
            bool success = _dungeonManager.MoveTo(direction);

            if (!success)
            {
                Debug.Log($"Uderzy³eœ w œcianê! Brak przejœcia na wektorze: {directionName}.");
            }
        }
    }
}