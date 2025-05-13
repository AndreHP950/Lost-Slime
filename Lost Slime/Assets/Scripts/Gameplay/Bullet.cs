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
        if (h == null)
            h = other.GetComponentInChildren<Health>(); // tenta no filho   
        if (h == null)
            h = other.GetComponentInParent<Health>(); // tenta no pai

        if (h != null)
        {
            // Se estiver imune, não aplica dano e não destrói a bala
            if (h.isImmune)
                return;

            h.Apply(-Mathf.RoundToInt(damage));
            Debug.Log($"{other.name} levou {damage} de dano, vida agora {h.Current}");

            // Só destrói a bala se causou dano
            Destroy(gameObject);
        }
        else
        {
            // Se colidir com qualquer outra coisa, destrói normalmente
            Destroy(gameObject);
        }
    }
}
