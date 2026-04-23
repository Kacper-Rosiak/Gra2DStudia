using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    public void SelectClass(PlayerClassData data)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectedClassData = data;
        }
        SceneManager.LoadScene("CampScene");
    }
}