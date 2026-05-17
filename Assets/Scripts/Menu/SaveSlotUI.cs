using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button slotButton;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI levelText;

    private int _slotIndex;
    private Action<int> _onSlotClicked;

    public void Setup(int index, SaveData data, Action<int> onClick)
    {
        _slotIndex = index;
        _onSlotClicked = onClick;

        // Pobieramy obecne ustawienia kolorów przycisku
        ColorBlock colors = slotButton.colors;

        if (data != null)
        {
            slotButton.interactable = true;
            dateText.text = data.timestamp;
            levelText.text = $"Level: {data.level}";
        }
        else
        {
            // Blokujemy klikanie, ale ustawiamy kolor "Disabled" na taki sam jak "Normal"
            // dzięki czemu tło nie będzie wyszarzone.
            slotButton.interactable = false;
            colors.disabledColor = colors.normalColor; 
            slotButton.colors = colors;

            dateText.text = "Pusty slot";
            levelText.text = "---";
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => _onSlotClicked?.Invoke(_slotIndex));
    }
}
