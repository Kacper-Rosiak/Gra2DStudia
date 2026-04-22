using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    public static PlayerClassData SelectedClassData;

    public void SelectClass(PlayerClassData data)
    {
        SelectedClassData = data;
        SceneManager.LoadScene("CombatScene");
    }
}