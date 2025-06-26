using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Configuração de Vida")]
    [SerializeField] private int maxHealth = 5;
    public int MaxHealth => maxHealth;
    public int Current { get; private set; }

    // Eventos para notificar alterações na vida, hits e morte
    public UnityEvent<int> onHealthChanged;
    public UnityEvent onDied;
    public UnityEvent onHit;
    public bool isImmune = false;

    [Header("Animation")]
    [SerializeField] private Animator healthAnimator; // Para disparar triggers de animação

    void Awake()
    {
        Current = maxHealth;
        onHealthChanged?.Invoke(Current);

        if (healthAnimator == null)
            healthAnimator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Aplica dano (valor negativo) ou cura (valor positivo) e dispara os eventos correspondentes.
    /// </summary>
    public void Apply(int amount)
    {
        if (amount < 0 && isImmune)
            return;

        if (Current <= 0)
            return;

        int oldCurrent = Current;
        Current = Mathf.Clamp(Current + amount, 0, maxHealth);
        onHealthChanged?.Invoke(Current);

        if (amount < 0)
        {
            onHit?.Invoke();
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHitLowPitch();
            if (healthAnimator != null)
                healthAnimator.SetTrigger("Hit");

            if (oldCurrent > 0 && Current == 0)
            {
                if (healthAnimator != null)
                    healthAnimator.SetTrigger("Death");
                onDied?.Invoke();
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
}
