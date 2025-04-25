// Assets/Scripts/Gameplay/Bullet.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Configuração da Bala")]
    [Tooltip("Dano que esta bala causa ao atingir um inimigo")]
    public float damage = 1f;
    [Tooltip("Tempo em segundos antes de auto-destruir")]
    public float lifeTime = 3f;

    Rigidbody rb;
    float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // para usar OnTriggerEnter, deixe o Collider como trigger
        GetComponent<Collider>().isTrigger = true;
        rb.useGravity = false;
    }

    void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        // acumula tempo de vida
        timer += Time.deltaTime;
        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        /*
         * // se atingir algo com Enemy, subtrai health
        var enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.health -= damage;
        }
        */
        // desaparece ao colidir em qualquer coisa
        Destroy(gameObject);
    }
}
