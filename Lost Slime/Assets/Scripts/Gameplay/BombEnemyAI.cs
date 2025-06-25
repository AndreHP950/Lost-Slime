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

    [Header("Refer�ncias")]
    [SerializeField] private Renderer bombRenderer;

    [Header("�udio da Bomba")]
    [SerializeField] private AudioClip beepExplosionClip;

    [Header("Efeito de Explos�o")]
    [SerializeField] private VisualEffectAsset explosionVFX;

    private NavMeshAgent agent;
    private Transform player;
    private bool isChasing = false;
    private bool isExploding = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        agent.updateRotation = false;

        player = GameObject.Find("Player")?.transform;

        if (bombRenderer == null)
        {
            var bomb = transform.Find("BombEnemy");
            if (bomb != null)
                bombRenderer = bomb.GetComponent<Renderer>();
        }
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
            agent.SetDestination(transform.position);
        }

        // Rotaciona para a dire��o do movimento
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

        // Toca o �udio de persegui��o
        if (AudioManager.Instance != null && beepExplosionClip != null)
            AudioManager.Instance.PlaySfx(beepExplosionClip);

        while (timer < explosionDelay)
        {
            if (player != null)
            {
                Vector3 target = player.position;
                target.y = transform.position.y;
                agent.SetDestination(target);
            }

            yield return null;
            timer += Time.deltaTime;
        }

        isExploding = true;
        agent.isStopped = true;

        // Aplica dano em �rea
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

        // Armazena a posi��o para criar o efeito depois
        Vector3 explosionPosition = transform.position;

        // PRIMEIRO: Destroi a bomba
        Destroy(gameObject);

        // DEPOIS: Cria o efeito de explos�o (usando GameObject.CreatePrimitive como container)
        if (explosionVFX != null)
        {
            // Cria um GameObject vazio para hospedar o VFX
            GameObject vfxObject = new GameObject("ExplosionVFX");
            vfxObject.transform.position = explosionPosition;

            // Adiciona o componente VisualEffect e configura o asset
            VisualEffect vfx = vfxObject.AddComponent<VisualEffect>();
            vfx.visualEffectAsset = explosionVFX;

            // Envia o evento para iniciar a explos�o
            vfx.SendEvent("Explode");

            // Destroi o objeto VFX ap�s 3 segundos
            Destroy(vfxObject, 3f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
