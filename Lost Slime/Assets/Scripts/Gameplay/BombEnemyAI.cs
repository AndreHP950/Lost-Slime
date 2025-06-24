using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.VFX;

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

    [Header("�udio da Bomba")]
    [SerializeField] private AudioClip beepExplosionClip; // �udio �nico com beeps e explos�o

    [Header("Efeito de Explos�o")]
    [SerializeField] private GameObject explosionVFX; // Prefab do efeito de explos�o (VFX Graph ou outro)

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

        // Tenta encontrar automaticamente o Renderer do filho "BombEnemy"
        if (bombRenderer == null)
        {
            var bomb = transform.Find("BombEnemy");
            if (bomb != null)
                bombRenderer = bomb.GetComponent<Renderer>();
        }

        // Tenta encontrar o ParticleSystem no filho "WarningParticles"
        if (warningParticles == null)
        {
            var warningObj = transform.Find("WarningParticles");
            if (warningObj != null)
                warningParticles = warningObj.GetComponent<ParticleSystem>();
        }

        // Garante que o ParticleSystem esteja parado no in�cio
        if (warningParticles != null)
            warningParticles.Stop();

        // Define um valor padr�o para blinkColor se necess�rio (n�o utilizado aqui)
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

        // Ativa o ParticleSystem de aviso
        if (warningParticles != null)
            warningParticles.Play();

        // Toca o �udio �nico (beeps seguidos da explos�o) assim que a bomba � acionada
        if (AudioManager.Instance != null && beepExplosionClip != null)
            AudioManager.Instance.PlaySfx(beepExplosionClip);

        while (timer < explosionDelay)
        {
            // Anda reto at� o player (mantendo a mesma altura)
            Vector3 target = player.position;
            target.y = transform.position.y;
            agent.SetDestination(target);

            // Modula o ParticleSystem: aumenta a taxa de emiss�o com o tempo
            if (warningParticles != null)
            {
                var em = warningParticles.emission;
                em.rateOverTime = Mathf.Lerp(5f, 50f, timer / explosionDelay);
            }

            yield return null;
            timer += Time.deltaTime;
        }

        // Desliga o ParticleSystem
        if (warningParticles != null)
            warningParticles.Stop();

        // Procede com a explos�o
        isExploding = true;
        agent.isStopped = true;
        Explode();
    }

    private void Explode()
    {
        // Instancia o efeito de explos�o na posi��o da bomba
        if (explosionVFX != null)
        {
            GameObject explosionEffect = Instantiate(explosionVFX, transform.position, transform.rotation);

            // Obt�m o componente VisualEffect do prefab
            VisualEffect vfx = explosionEffect.GetComponent<VisualEffect>();
            if (vfx != null)
            {
                
                 vfx.SendEvent("Explode");
            }

            Destroy(explosionEffect, 3f); // Destroi o efeito ap�s 3 segundos (ajuste conforme necess�rio)
        }

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
