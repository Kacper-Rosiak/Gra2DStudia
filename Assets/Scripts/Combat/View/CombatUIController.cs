using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CombatUIController : MonoBehaviour
{
    [Header("Menu Akcji")]
    // To jest to brakujące ogniwo, o które prosił CombatController
    public GameObject actionMenu;

    [Header("Paski HP")]
    public Slider playerSlider;
    public Slider enemySlider;

    [Header("Teksty")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI combatLogText;

    public void InitializeUI(Entity player, Entity enemy)
    {
        // Podpinamy się pod sygnały zmiany zdrowia
        player.OnHealthChanged += (curr, max) => StartCoroutine(AnimateBar(playerSlider, curr, max));
        enemy.OnHealthChanged += (curr, max) => StartCoroutine(AnimateBar(enemySlider, curr, max));

        // Ustawiamy paski na aktualne wartości przy starcie
        if (playerSlider != null) playerSlider.value = (float)player.CurrentHP / player.MaxHP;
        if (enemySlider != null) enemySlider.value = (float)enemy.CurrentHP / enemy.MaxHP;

        // Na starcie ukrywamy menu (CombatController je włączy w turze gracza)
        if (actionMenu != null) actionMenu.SetActive(false);
    }

    // MAGIA: Płynna animacja paska (Lerp)
    private IEnumerator AnimateBar(Slider slider, int current, int max)
    {
        float targetValue = (float)current / (float)max;
        float startValue = slider.value;
        float time = 0;
        float duration = 0.5f;

        while (time < duration)
        {
            time += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, time / duration);
            yield return null;
        }
        slider.value = targetValue;
    }

    // Metody wywoływane przez Managera do aktualizacji UI
    public void UpdateTurnText(string status) => turnText.text = "Status: " + status;
    public void ShowMessage(string message) => combatLogText.text = message;
}