using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public TextMeshProUGUI errorText;

    private void Start()
    {
        if (errorText != null) errorText.gameObject.SetActive(false);
    }

    public void SelectClass(PlayerClassData data)
    {
        // Walidacja nazwy
        string enteredName = nameInputField != null ? nameInputField.text : "";

        if (string.IsNullOrWhiteSpace(enteredName))
        {
            if (errorText != null)
            {
                errorText.text = "Wpisz nazwę postaci!";
                errorText.gameObject.SetActive(true);
            }
            Debug.LogWarning("CharacterSelection: Nie wpisano nazwy gracza!");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectedClassData = data;
            GameManager.Instance.SelectedPlayerName = enteredName;
        }

        SceneManager.LoadScene("CampScene");
    }
}
