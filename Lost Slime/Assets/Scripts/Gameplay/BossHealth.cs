using UnityEngine;
using UnityEngine.Events;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 30;
    public int currentHealth;

    // Evento disparado quando a vida muda (passa a vida atual)
    public UnityEvent<int> onHealthChanged;
    // Evento disparado quando o boss morre
    public UnityEvent onBossDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    // Chame este método para aplicar dano ao boss
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;
        if (onHealthChanged != null)
            onHealthChanged.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            if (onBossDied != null)
                onBossDied.Invoke();
            // Aqui você pode adicionar animação, som de morte, etc.
            gameObject.SetActive(false); // Desativa o boss
        }
    }
}
