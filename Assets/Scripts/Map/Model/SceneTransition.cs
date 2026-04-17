using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    private string sceneToLoad = "DungeonScene";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Upewnij siê, ¿e obiekt, który wszed³ w trigger, to gracz (np. sprawdzaj¹c Tag)
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}