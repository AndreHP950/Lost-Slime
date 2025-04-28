using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Configura��o do Pickup")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private Color flashColor = Color.green;
    [SerializeField] private float flashDuration = 0.2f;

    [Header("Refer�ncias")]
    [Tooltip("Arraste aqui o componente Health do Player")]
    [SerializeField] private Health targetHealth;

    private HitFlash targetFlash;

    void Awake()
    {
        if (targetHealth == null)
            Debug.LogError("HealthPickup: arraste o componente Health do Player no Inspector!");

        targetFlash = targetHealth.GetComponent<HitFlash>();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Pickup detectou: " + other.name);
        if (!other.CompareTag("Player")) return;

        // pega o Health do player
        var hp = other.GetComponent<Health>();
        if (hp == null)
            return;

        // se j� estiver com vida cheia, n�o faz nada
        if (hp.Current >= hp.MaxHealth)
            return;

        // Aplica cura
        targetHealth.Apply(+healAmount);

        // Flash verde r�pido
        if (targetFlash != null)
        {
            // salva valores antigos
            var prevColor = targetFlash.HitColor;
            var prevDur = targetFlash.FlashDuration;

            targetFlash.HitColor = flashColor;
            targetFlash.FlashDuration = flashDuration;
            targetFlash.ForceFlash();

            // restaura valores
            targetFlash.HitColor = prevColor;
            targetFlash.FlashDuration = prevDur;
        }

        // destr�i a caixinha
        Destroy(gameObject);
    }
}
