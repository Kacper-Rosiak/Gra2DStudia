using UnityEngine;
using UnityEngine.SceneManagement; 

public class KeepMusicPlaying : MonoBehaviour
{
    private static KeepMusicPlaying instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "CampScene" || scene.name == "DungeonScene")
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}