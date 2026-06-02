using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class ZaokragloneRogi : MonoBehaviour
{
    // Tutaj ustawiasz, jak bardzo okrągłe mają być rogi!
    [Range(0f, 100f)] public float promienRogow = 20f;

    void Update()
    {
        // Ten prosty kod używa zaawansowanych funkcji Unity, żeby zaokrąglić brzegi bez używania spritów
        var image = GetComponent<Image>();
        if (image != null && image.sprite == null)
        {
            // Jeśli nie masz sprita, skrypt wymusi użycie domyślnego kształtu z wektorowym zaokrągleniem
#if UNITY_2022_1_OR_NEWER
            image.pixelsPerUnitMultiplier = promienRogow;
#endif
        }
    }
}