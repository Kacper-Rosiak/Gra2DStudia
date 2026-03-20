using UnityEngine;
using UnityEngine.UI;
using TMPro; // Zak³adam u¿ycie TextMeshPro

public class CombatUIView : MonoBehaviour
{
    public Button specialAbilityButton;
    public TextMeshProUGUI abilityNameText;

    private Player _player;

    public void SetupUI(Player player)
    {
        _player = player;

        if (_player.SpecialAbility != null)
        {
            abilityNameText.text = _player.SpecialAbility.AbilityName;

            // Nas³uchiwanie na eventy z modelu bez ³amania separacji
            _player.SpecialAbility.OnAbilityVisualsTriggered += PlayAbilityVFX;

            specialAbilityButton.onClick.AddListener(OnAbilityButtonClicked);
        }
        else
        {
            specialAbilityButton.interactable = false;
        }
    }

    private void OnAbilityButtonClicked()
    {
        // Tutaj wyœlesz sygna³ do CombatManagera, aby u¿y³ UseAbilityCommand
        Debug.Log("Zdolnoœæ klikniêta w UI!");
    }

    private void PlayAbilityVFX(string vfxId)
    {
        // Tutaj np. switch lub s³ownik odpalaj¹cy Particle System w Unity
        Debug.Log($"<color=magenta>[Widok]</color> Odtwarzam efekt wizualny: {vfxId}");
    }

    private void OnDestroy()
    {
        if (_player != null && _player.SpecialAbility != null)
        {
            _player.SpecialAbility.OnAbilityVisualsTriggered -= PlayAbilityVFX;
        }
    }
}