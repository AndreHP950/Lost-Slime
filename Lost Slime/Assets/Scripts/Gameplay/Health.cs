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

    void Awake()
    {
        Current = maxHealth;
        onHealthChanged.Invoke(Current);
    }

    /// <summary>
    /// amount < 0 = dano, > 0 = cura
    /// </summary>
    public void Apply(int amount)
    {
        // Só ignora se for dano e estiver imune
        if (amount < 0 && isImmune) return;
        if (Current <= 0) return;

        Current = Mathf.Clamp(Current + amount, 0, maxHealth);
        onHealthChanged.Invoke(Current);

        if (amount < 0)
        {
            onHit.Invoke();
            // TOCA SOM DE DANO
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayDamage();

            if (Current == 0)
            {
                // TOCA SOM DE MORTE
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayDeath();
                TriggerDeathEffect(); // Adiciona o efeito de morte
                onDied.Invoke();
            }
        }
        else if (amount > 0)
        {
            // TOCA SOM DE CURA
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHeal();
        }
    }

    private void TriggerDeathEffect()
    {
        if (deathEffectPrefab != null)
        {
            // Instancia o efeito na posição do inimigo
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            Destroy(effect, 3f); // Destroi o efeito após 3 segundos
        }
    }
}

