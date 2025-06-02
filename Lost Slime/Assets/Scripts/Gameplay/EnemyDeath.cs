using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeath : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Health>()
           .onDied.AddListener(OnEnemyDied);
    }

    private void OnEnemyDied()
    {
        // TOCA SOM DE MORTE DE INIMIGO
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDeath();
        Destroy(gameObject);
    }
}
