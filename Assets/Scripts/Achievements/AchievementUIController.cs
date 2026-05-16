using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AchievementUIController : MonoBehaviour
{
    public GameObject popupPrefab; // Prefab z Canvas Group
    public Transform spawnPoint;   // Canvas Overlay

    // --- NOWE USTAWIENIA ---
    [Header("Ustawienia Wielkości")]
    public float maxAllowedWidth = 600f;  // Maksymalna szerokość na ekranie
    public float maxAllowedHeight = 200f; // Maksymalna wysokość na ekranie
    // ------------------------

    private Queue<Sprite> queue = new Queue<Sprite>();
    private bool isShowing = false;

    public void ShowAchievement(Sprite achievementSprite)
    {
        if (achievementSprite == null) return;
        queue.Enqueue(achievementSprite);
        if (!isShowing) StartCoroutine(DisplayNext());
    }

    IEnumerator DisplayNext()
    {
        isShowing = true;
        while (queue.Count > 0)
        {
            Sprite currentSprite = queue.Dequeue();
            GameObject popup = Instantiate(popupPrefab, spawnPoint);
            Image img = popup.GetComponent<Image>();

            // 1. Najpierw ustawiamy Native Size, żeby zachować proporcje
            img.sprite = currentSprite;
            img.SetNativeSize();

            // 2. NOWA LOGIKA: Inteligentne skalowanie
            // Pobieramy rozmiar, jaki Unity chce ustawić (Native Size)
            Vector2 nativeSize = img.rectTransform.sizeDelta;

            // Obliczamy skale dla szerokości i wysokości
            float scaleW = maxAllowedWidth / nativeSize.x;
            float scaleH = maxAllowedHeight / nativeSize.y;

            // Wybieramy mniejszą skalę (Match Height or Width), żeby obrazek 
            // zmieścił się w "pudełku" maxW x maxH, nie tracąc proporcji.
            float finalScale = Mathf.Min(1f, Mathf.Min(scaleW, scaleH));

            // Nakładamy skalę na obiekt
            img.rectTransform.sizeDelta = nativeSize * finalScale;

            // 3. Reszta bez zmian
            yield return new WaitForSeconds(3f);
            Destroy(popup);
            yield return new WaitForSeconds(0.5f);
        }
        isShowing = false;
    }
    void Update()
    {
        // Jeśli naciśniesz T w czasie gry
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Naciśnięto T - próba pokazania osiągnięcia!");

            // Próbujemy pobrać grafikę ze skryptu obok
            GameAchievementTriggers triggers = GetComponent<GameAchievementTriggers>();

            if (triggers != null && triggers.grafikaKill != null)
            {
                ShowAchievement(triggers.grafikaKill);
            }
            else
            {
                Debug.LogWarning("Nie mogę pokazać testu: Sprawdź czy w GameAchievementTriggers masz wrzucone PNG!");
            }
        }
    }
}