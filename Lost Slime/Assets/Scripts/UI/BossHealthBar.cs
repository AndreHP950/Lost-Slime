using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Image bossHealthBarFill;

    private Health bossHealth;

    void Start()
    {
        GameObject boss = GameObject.FindWithTag("Boss");
        if (boss != null)
        {
            bossHealth = boss.GetComponent<Health>();
            if (bossHealth != null)
            {
                bossHealth.onHealthChanged.AddListener(UpdateHealthBar);
                UpdateHealthBar(bossHealth.Current);
            }
        }
        else
        {
            Debug.LogWarning("Boss com tag 'Boss' não encontrado na cena.");
        }
    }

    void UpdateHealthBar(int currentHealth)
    {
        if (bossHealth != null && bossHealthBarFill != null)
        {
            bossHealthBarFill.fillAmount = (float)currentHealth / bossHealth.MaxHealth;
        }
    }
}
