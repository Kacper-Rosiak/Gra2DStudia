using UnityEngine;
using System.Collections.Generic;

public class CombatController : MonoBehaviour
{
    [Header("Referencje UI")]
    [SerializeField] private CombatUIController uiController; // Przeciągnij tu swój _UI_Manager w Inspektorze

    private CombatManager _combatManager;

    // Przechowujemy zawodników, żeby przyciski wiedziały kogo dotyczy akcja
    private Entity _player;
    private Entity _enemy;

    void Start()
    {
        // Tworzymy silnik walki (Model)
        _combatManager = new CombatManager();

        // Sprawdzenie, czy przypisałeś UI w Inspektorze
        if (uiController == null)
        {
            Debug.LogError("CombatController: Nie przypisano CombatUIController (UI Manager)!");
        }
    }

    /// <summary>
    /// Metoda wywoływana przez system gry (np. GameManagera) w celu rozpoczęcia walki.
    /// </summary>
    public void SetupBattle(Entity player, Entity enemy)
    {
        if (player == null || enemy == null)
        {
            Debug.LogError("CombatController: Próba startu walki bez zawodników!");
            return;
        }

        _player = player;
        _enemy = enemy;

        // 1. Podpinamy logi (Manager przesyła tekst -> UI go wyświetla)
        _combatManager.OnCombatLog += uiController.ShowMessage;

        // 2. Obsługa zmiany tur (aktualizacja tekstu i włączanie/wyłączanie menu)
        _combatManager.OnStateChanged += (state) => {
            uiController.UpdateTurnText(state.ToString());

            if (uiController.actionMenu != null)
            {
                // Menu przycisków pojawia się TYLKO w turze gracza
                bool isPlayerTurn = (state == BattleState.PlayerTurn);
                uiController.actionMenu.SetActive(isPlayerTurn);
            }
        };

        // 3. Inicjalizacja wizualna pasków HP w Twoim UI
        uiController.InitializeUI(_player, _enemy);

        // 4. Rozpoczęcie sekwencji walki
        List<Entity> participants = new List<Entity> { _player, _enemy };
        _combatManager.StartBattle(participants);
    }

    // --- METODY DLA PRZYCISKÓW (On Click w Unity) ---

    public void OnAttackButtonClicked()
    {
        // Sprawdzamy czy to tura gracza
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;

        // Zabezpieczenie przed brakiem danych
        if (_player == null || _enemy == null) return;

        Debug.Log("UI: Kliknięto przycisk ATAK");

        // Tworzymy komendę ataku (zgodnie z systemem kolegów)
        // message => uiController.ShowMessage(message) przesyła opis ataku do logu walki
        ICombatCommand attack = new AttackCommand(_player, _enemy, message => uiController.ShowMessage(message));

        // Wykonujemy akcję w silniku walki
        _combatManager.ExecuteTurnAction(attack);
    }

    public void OnDefendButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;

        uiController.ShowMessage("Bohater przyjmuje postawę obronną!");

        // Tutaj można wywołać DefendCommand, jeśli jest zaimplementowany:
        // _combatManager.ExecuteTurnAction(new DefendCommand(_player));
    }

    public void OnEscapeButtonClicked()
    {
        if (_combatManager == null || _combatManager.CurrentState != BattleState.PlayerTurn) return;

        uiController.ShowMessage("Próba ucieczki...");
        _combatManager.TryEscape(40); // 40% szansy na ucieczkę
    }
}