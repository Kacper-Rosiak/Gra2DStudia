using UnityEngine;

/// <summary>
/// Skrypt pomocniczy, który sprawia, że obiekt nie jest niszczony przy zmianie sceny.
/// Idealny dla NPC, którzy mają być widoczni wszędzie lub menedżerów.
/// </summary>
public class PersistentObject : MonoBehaviour
{
    [SerializeField] private bool destroyIfDuplicate = true;
    private static System.Collections.Generic.Dictionary<string, PersistentObject> _instances = new System.Collections.Generic.Dictionary<string, PersistentObject>();

    private void Awake()
    {
        if (destroyIfDuplicate)
        {
            string id = gameObject.name; // Można użyć bardziej unikalnego ID
            if (_instances.ContainsKey(id) && _instances[id] != this)
            {
                Destroy(gameObject);
                return;
            }
            _instances[id] = this;
        }

        DontDestroyOnLoad(gameObject);
    }
}
