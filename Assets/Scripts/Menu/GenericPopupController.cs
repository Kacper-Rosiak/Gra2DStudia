using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class GenericPopupController : MonoBehaviour
{
    public static GenericPopupController Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button okButton;

    private Action _onConfirmAction;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (popupPanel != null) popupPanel.SetActive(false);
        if (okButton != null) okButton.onClick.AddListener(OnOkButtonClicked);
    }

    public void ShowPopup(string title, string message, Action onConfirm = null)
    {
        Debug.Log($"[GenericPopup] Wywołano popup: {title}");
        if (popupPanel == null)
        {
            Debug.LogError("[GenericPopup] popupPanel jest NULLEM!");
            return;
        }

        titleText.text = title;
        messageText.text = message;
        _onConfirmAction = onConfirm;

        popupPanel.SetActive(true);
        
        // Wymuszamy widoczność myszki
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Upewniamy się, że GraphicRaycaster jest włączony na wszystkich Canvasach w popu-pie
        var raycasters = GetComponentsInChildren<GraphicRaycaster>(true);
        foreach (var gr in raycasters) 
        {
            gr.enabled = true;
            Debug.Log($"[GenericPopup] Włączono GraphicRaycaster na {gr.gameObject.name}");
        }

        // Upewniamy się, że Canvas jest w trybie Overlay i na samym wierzchu
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767; // Maksymalna wartość short
            Debug.Log($"[GenericPopup] Canvas ustawiony na Overlay, SortingOrder: {canvas.sortingOrder}");
        }

        // Upewniamy się, że EventSystem działa
        if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.enabled)
        {
            var es = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es != null) 
            {
                es.enabled = true;
                Debug.Log("[GenericPopup] Włączono istniejący EventSystem.");
            }
            else
            {
                GameObject esObj = new GameObject("GenericPopup_EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[GenericPopup] Stworzono nowy EventSystem.");
            }
        }

        Time.timeScale = 0f; // Pauza
    }

    private void OnOkButtonClicked()
    {
        Debug.Log("[GenericPopup] Kliknięto przycisk OK");
        Time.timeScale = 1f; // Unpause
        popupPanel.SetActive(false);

        var action = _onConfirmAction;
        _onConfirmAction = null;
        action?.Invoke();
    }
}
