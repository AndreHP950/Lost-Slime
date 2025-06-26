using UnityEngine;

public class BossPagePickup : MonoBehaviour
{
    [Header("Efeitos Visuais do Pickup")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 1f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Rotação contínua
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        // Efeito de flutuação (bob) que garante não ficar abaixo de startPosition.y
        float bobOffset = (Mathf.Sin(Time.time * bobSpeed) + 1f) * (bobHeight / 2f);
        transform.position = new Vector3(startPosition.x, startPosition.y + bobOffset, startPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter com: " + other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entrou no trigger do BossPagePickup.");
            if (BossPageManager.Instance != null)
            {
                BossPageManager.Instance.ShowBossPage();
            }
            else
            {
                Debug.LogWarning("BossPageManager não encontrado.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit com: " + other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player saiu do trigger do BossPagePickup.");
            if (BossPageManager.Instance != null)
            {
                BossPageManager.Instance.HideBossPage();
            }
        }
    }
}
