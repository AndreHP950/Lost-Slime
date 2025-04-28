using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Configuração de Vida")]
    [SerializeField] private int maxHealth = 5;
    public int MaxHealth => maxHealth;
    public int Current { get; private set; }

    // Eventos pra UI / efeitos / morte
    public UnityEvent<int> onHealthChanged;
    public UnityEvent onDied;
    public UnityEvent onHit;

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
        if (Current <= 0) return;

        Current = Mathf.Clamp(Current + amount, 0, maxHealth);
        onHealthChanged.Invoke(Current);

        if (amount < 0)
        {
            onHit.Invoke();
            if (Current == 0)
                onDied.Invoke();
        }
    }
}
