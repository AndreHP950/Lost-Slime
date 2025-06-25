using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Configuração do Pickup")]
    [SerializeField] private int healAmount = 1;
    private Color flashColor = new Color(0f, 0.3f, 0f, 1f);
    [SerializeField] private float flashDuration = 0.2f;

    [Header("Referências")]
    [Tooltip("Arraste aqui o componente Health do Player")]
    private Health targetHealth;

    private HitFlash targetFlash;

    [Header("Efeitos Visuais")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 1f;

    private Vector3 startPosition;

    void Awake()
    {
        // Procura o objeto com a tag "Player" e pega o Health
        if (targetHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                targetHealth = player.GetComponent<Health>();
        }

        targetFlash = targetHealth != null ? targetHealth.GetComponent<HitFlash>() : null;
        startPosition = transform.position;
    }


    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var hp = other.GetComponent<Health>();
        if (hp == null)
            return;

        if (hp.Current >= hp.MaxHealth)
            return;

        targetHealth.Apply(+healAmount);

        if (targetFlash != null)
        {
            var prevColor = targetFlash.HitColor;
            var prevDur = targetFlash.FlashDuration;

            targetFlash.HitColor = flashColor;
            targetFlash.FlashDuration = flashDuration;
            targetFlash.ForceFlash();

            targetFlash.HitColor = prevColor;
            targetFlash.FlashDuration = prevDur;
        }

        Destroy(gameObject);
    }
}
