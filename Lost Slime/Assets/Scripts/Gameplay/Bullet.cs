using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Configuração da Bala")]
    public float damage = 1f;
    public float lifeTime = 3f;

    private Rigidbody rb;
    private float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<Collider>().isTrigger = true;
        rb.useGravity = false;
    }

    void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1) tenta aplicar dano em quem tiver Health
        var h = other.GetComponent<Health>();
        if (h != null)
        {
            h.Apply(-Mathf.RoundToInt(damage));
            Debug.Log($"{other.name} levou {damage} de dano, vida agora {h.Current}");
        }

        // 2) destrói a bala
        Destroy(gameObject);
    }
}
