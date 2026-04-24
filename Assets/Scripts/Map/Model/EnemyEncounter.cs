using UnityEngine;

[RequireComponent(typeof(EnemyOnMap))]
public class EnemyEncounter : MonoBehaviour
{
    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_triggered) return;

        if (collision.CompareTag("Player"))
        {
            _triggered = true;
            Debug.Log($"EnemyEncounter: Gracz wykryty! Rozpoczynanie walki z {gameObject.name}");
            
            if (CombatTransitionManager.Instance != null)
            {
                CombatTransitionManager.Instance.StartCombat(collision.gameObject, gameObject);
            }
            else
            {
                Debug.LogError("EnemyEncounter: Brak instancji CombatTransitionManager na scenie!");
            }
        }
    }
}
