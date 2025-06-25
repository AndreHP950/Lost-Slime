using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Configura��o de Vida")]
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
    [SerializeField] private Animator healthAnimator;  // Refer�ncia ao Animator para disparar triggers

    void Awake()
    {
        Current = maxHealth;
        onHealthChanged.Invoke(Current);
        
        // Tenta obter o Animator se n�o tiver sido atribu�do
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

        // Se j� est� morto, n�o faz nada
        if (Current <= 0)
            return;

        int oldCurrent = Current;
        Current = Mathf.Clamp(Current + amount, 0, maxHealth);
        onHealthChanged.Invoke(Current);

        if (amount < 0)
        {
            onHit.Invoke();
            AudioManager.Instance.PlayHitLowPitch();
            if (healthAnimator != null)
                healthAnimator.SetTrigger("Hit");

            // S� dispara morte se estava vivo antes e agora morreu
            if (oldCurrent > 0 && Current == 0)
            {
                if (healthAnimator != null)
                    healthAnimator.SetTrigger("Death");

                onDied.Invoke();
                TriggerDeathEffect();
            }
        }
        else if (amount > 0)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHeal();
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
