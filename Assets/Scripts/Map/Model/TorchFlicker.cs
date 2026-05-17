using UnityEngine;
using UnityEngine.Rendering.Universal; // Wymagane do obs³ugi Light 2D

[RequireComponent(typeof(Light2D))]
public class TorchFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [Tooltip("Prêdkoœæ migotania ognia")]
    [SerializeField] private float flickerSpeed = 3f;

    [Tooltip("Jak bardzo zmienia siê zasiêg œwiat³a")]
    [SerializeField] private float radiusAmount = 0.5f;

    [Tooltip("Jak bardzo zmienia siê jasnoœæ (intensywnoœæ) œwiat³a")]
    [SerializeField] private float intensityAmount = 0.2f;

    private Light2D _light;
    private float _baseRadius;
    private float _baseIntensity;
    private float _randomOffset;

    private void Start()
    {
        _light = GetComponent<Light2D>();

        // Zapisujemy bazowe ustawienia œwiat³a z Inspektora
        _baseRadius = _light.pointLightOuterRadius;
        _baseIntensity = _light.intensity;

        // Unikalne przesuniêcie, aby ka¿da pochodnia migota³a w swoim w³asnym tempie
        _randomOffset = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        // Szum Perlina zwraca bardzo p³ynne, losowe wartoœci od 0.0 do 1.0. 
        // Skalujemy je do przedzia³u od -1.0 do 1.0, ¿eby œwiat³o ros³o i mala³o wokó³ bazy.
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + _randomOffset, 0f) * 2f - 1f;

        // Aplikujemy zmiany do œwiat³a
        _light.pointLightOuterRadius = _baseRadius + (noise * radiusAmount);
        _light.intensity = _baseIntensity + (noise * intensityAmount);
    }
}