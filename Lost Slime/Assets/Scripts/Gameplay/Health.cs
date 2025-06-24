using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Configuração de Vida")]
    [SerializeField] private int maxHealth = 5;
    public int MaxHealth => maxHealth;
    public int Current { get; private set; }

    [Header("Efeito de Morte")]
    [SerializeField] private GameObject deathEffectPrefab; // Prefab do Visual Effect Graph

    // Eventos pra UI / efeitos / morte
    public UnityEvent<int> onHealthChanged;
    public UnityEvent onDied;
    public UnityEvent onHit;
    public bool isImmune = false; // Flag to track damage immunity

    [Header("Animation")]
    [SerializeField] private Animator healthAnimator;  // Referência ao Animator para disparar triggers

    void Awake()
    {
        Current = maxHealth;
        onHealthChanged.Invoke(Current);
        
        // Tenta obter o Animator se não tiver sido atribuído
        if (healthAnimator == null)
            healthAnimator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// amount < 0 = dano, > 0 = cura
    /// </summary>
    public void Apply(int amount)
    {
        // Ignora dano se estiver imune
        if (amount < 0 && isImmune)
            return;

        if (Current <= 0)
            return;

        Current = Mathf.Clamp(Current + amount, 0, maxHealth);
        onHealthChanged.Invoke(Current);

        if (amount < 0)
        {
            onHit.Invoke();
            AudioManager.Instance.PlayHitLowPitch();
            // Dispara trigger Hit
            if (healthAnimator != null)
                healthAnimator.SetTrigger("Hit");

            if (Current == 0)
            {
                // Dispara trigger Death
                if (healthAnimator != null)
                    healthAnimator.SetTrigger("Death");

                onDied.Invoke();
                // Opcional: Chame o efeito de morte.
                TriggerDeathEffect();
            }
        }
        else if (amount > 0)
        {
            // TOCA SOM DE CURA e se desejar, dispare um trigger para Recover
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHeal();
            // Exemplo:
            if (healthAnimator != null)
                healthAnimator.SetTrigger("Recover");
        }
    }

    private void TriggerDeathEffect()
    {
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            Destroy(effect, 3f);
        }
    }
}
