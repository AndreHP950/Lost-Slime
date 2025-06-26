using UnityEngine;

public class BossActivationTrigger : MonoBehaviour
{
    [SerializeField] private GameObject boss; // Arraste o Boss aqui no Inspector
    [SerializeField] private GameObject bossHealthBar;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (boss != null)
                boss.SetActive(true);

            // Ativa a barra de vida do boss
   
            if (bossHealthBar != null)
                bossHealthBar.SetActive(true);
            else
                Debug.LogWarning("BossHealthBar not found in the scene.");
            Destroy(gameObject); // Remove o trigger após ativar
        }
    }
}
