using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class HealthBar : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Prefab do segmento (uma Image simples)")]
    [SerializeField] private GameObject segmentPrefab;
    [Tooltip("Container para os segmentos (HealthBar GameObject)")]
    [SerializeField] private Transform segmentContainer;

    private Health health;
    private Image[] segments;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void Start()
    {
        // 1) Aloca array de segmentos do tamanho do maxHealth
        segments = new Image[health.MaxHealth];

        // 2) Instancia cada segmento como filho de segmentContainer
        for (int i = 0; i < health.MaxHealth; i++)
        {
            GameObject go = Instantiate(segmentPrefab, segmentContainer);
            segments[i] = go.GetComponent<Image>();
        }

        // 3) Se inscreve no evento de mudança de vida
        health.onHealthChanged.AddListener(UpdateBar);

        // 4) Atualiza a barra no estado inicial
        UpdateBar(health.Current);
    }

    private void UpdateBar(int currentHealth)
    {
        // Para cada segmento, habilita apenas se i < currentHealth
        for (int i = 0; i < segments.Length; i++)
            segments[i].enabled = (i < currentHealth);
    }
}
