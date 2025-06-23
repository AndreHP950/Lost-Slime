using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(SphereCollider), typeof(Health))]
public class BombEnemyAI : MonoBehaviour
{
    [Header("Configura��o")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float explosionRange = 2f;
    [SerializeField] private float explosionDelay = 3f;
    [SerializeField] private int explosionDamage = 2;
    [SerializeField] private Color blinkColor = default; // Valor n�o utilizado nessa abordagem

    [Header("Refer�ncias")]
    [SerializeField] private Renderer bombRenderer; // Pode ser usado para outros efeitos, se desejar
    [SerializeField] private ParticleSystem warningParticles; // Particle system de aviso

    private NavMeshAgent agent;
    private Transform player;
    private bool isChasing = false;
    private bool isExploding = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        agent.updateRotation = false;
        player = GameObject.Find("Player").transform;

        // Se n�o setado no Inspector, tenta encontrar automaticamente o Renderer do filho "BombEnemy"
        if (bombRenderer == null)
        {
            var bomb = transform.Find("BombEnemy");
            if (bomb != null)
                bombRenderer = bomb.GetComponent<Renderer>();
        }

        // Se n�o setado, tenta encontrar o ParticleSystem no filho "WarningParticles"
        if (warningParticles == null)
        {
            var warningObj = transform.Find("WarningParticles");
            if (warningObj != null)
                warningParticles = warningObj.GetComponent<ParticleSystem>();
        }

        // Garante que o ParticleSystem esteja parado no in�cio
        if (warningParticles != null)
            warningParticles.Stop();

        // Define um valor padr�o para blinkColor, se necess�rio (n�o � usado nessa abordagem)
        if (blinkColor == default)
            blinkColor = new Color(0.6f, 0f, 1f); // Roxo
    }

    void Update()
    {
        if (isExploding) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (!isChasing && dist <= detectionRange)
        {
            StartCoroutine(ChaseAndExplode());
        }
        else if (!isChasing)
        {
            agent.speed = patrolSpeed;
            agent.SetDestination(transform.position); // parado
        }

        // Rotaciona para a dire��o do movimento (apenas se estiver se movendo)
        Vector3 moveDir = agent.desiredVelocity;
        moveDir.y = 0f;
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir) * Quaternion.Euler(0, 180f, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private IEnumerator ChaseAndExplode()
    {
        isChasing = true;
        agent.speed = chaseSpeed;
        float timer = 0f;
        float blinkInterval = 1f; // come�a em 1 segundo

        // Ativa o sistema de part�culas de aviso
        if (warningParticles != null)
        {
            warningParticles.Play();
        }

        while (timer < explosionDelay)
        {
            // Anda reto at� o player (mantendo a mesma altura)
            Vector3 target = player.position;
            target.y = transform.position.y;
            agent.SetDestination(target);

            // Modula o ParticleSystem: aumente a taxa de emiss�o � medida que o tempo passa
            if (warningParticles != null)
            {
                var em = warningParticles.emission;
                em.rateOverTime = Mathf.Lerp(5f, 50f, timer / explosionDelay);

            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
            blinkInterval = Mathf.Max(blinkInterval / 2f, 0.05f); // nunca menor que 0.05s
        }

        // Desliga o sistema de part�culas
        if (warningParticles != null)
        {
            warningParticles.Stop();
        }

        // Explode
        isExploding = true;
        agent.isStopped = true;
        Explode();
    }

    private void Explode()
    {
        // Dano em �rea
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var health = hit.GetComponent<Health>();
                if (health != null)
                    health.Apply(-explosionDamage);
            }
        }
        // Adicione efeitos visuais ou part�culas de explos�o se desejar
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
