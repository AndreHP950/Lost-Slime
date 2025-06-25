using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private PowerUpType powerUpType;

    [Header("Efeitos Visuais")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 1f;

    [Header("Efeitos de Som")]
    [SerializeField] private AudioClip collectSound;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (PowerUpManager.Instance != null)
            {
                if (PowerUpManager.Instance.AddPowerUp(powerUpType))
                {
                    if (AudioManager.Instance != null && collectSound != null)
                        AudioManager.Instance.PlaySfx(collectSound);
                    Destroy(gameObject);
                }
            }
        }
    }
}
