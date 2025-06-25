using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeath : MonoBehaviour
{
    [Header("Power Up Drops")]
    [Tooltip("Chance de dropar um power up (0-1)")]
    [SerializeField] private float powerUpDropChance = 0.1f; // 10% de chance
    [SerializeField] private GameObject[] powerUpPrefabs; // Prefabs dos power ups

    void Awake()
    {
        GetComponent<Health>()
           .onDied.AddListener(OnEnemyDied);
    }

    private void OnEnemyDied()
    {
        // Toca som de morte
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDeath();

        // Chance de dropar um power up
        TryDropPowerUp();

        Destroy(gameObject);
    }

    private void TryDropPowerUp()
    {
        // Verifica se há prefabs configurados
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            return;

        // Verifica se o drop acontecerá
        if (Random.value <= powerUpDropChance)
        {
            // Escolhe um power up aleatório
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            GameObject powerUpPrefab = powerUpPrefabs[randomIndex];

            // Instancia o power up no local do inimigo
            if (powerUpPrefab != null)
            {
                Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
                Debug.Log("Inimigo dropou um power up!");
            }
        }
    }
}

