using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeath : MonoBehaviour
{
    [Header("Power Up Drops")]
    [Tooltip("Chance de dropar um power up (0-1)")]
    [SerializeField] private float powerUpDropChance = 0.1f;
    [SerializeField] private GameObject[] powerUpPrefabs;

    [Header("Health Drop")]
    [Tooltip("Chance de dropar vida (0-1)")]
    [SerializeField] private float healthDropChance = 0.2f;
    [SerializeField] private GameObject healthPickupPrefab;

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

        // Tenta dropar power up e vida (ambos podem acontecer)
        TryDropPowerUp();
        TryDropHealth();

        Destroy(gameObject);
    }

    private void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            return;

        if (Random.value <= powerUpDropChance)
        {
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            GameObject powerUpPrefab = powerUpPrefabs[randomIndex];

            if (powerUpPrefab != null)
            {
                // Offset para o power up (exemplo: para a direita)
                Vector3 offset = new Vector3(0.5f, 0, 0);
                Instantiate(powerUpPrefab, transform.position + offset, Quaternion.identity);
                Debug.Log("Inimigo dropou um power up!");
            }
        }
    }

    private void TryDropHealth()
    {
        if (healthPickupPrefab == null)
            return;

        if (Random.value <= healthDropChance)
        {
            // Offset para o health pickup (exemplo: para a esquerda)
            Vector3 offset = new Vector3(-0.5f, 0, 0);
            Instantiate(healthPickupPrefab, transform.position + offset, Quaternion.identity);
            Debug.Log("Inimigo dropou vida!");
        }
    }

}
