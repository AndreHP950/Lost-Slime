using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private Vector3 patrolAreaSize = new Vector3(10f, 1f, 10f);
    [SerializeField] private float minPatrolDistance = 2f;
    [SerializeField] private float waitTimeAtPoint = 2f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int bulletCount = 1;  // Shoot one bullet at a time for now
    [SerializeField] private float timeBetweenShots = 0.5f;  // Faster shooting rate

    [Header("Enemy Stats")]
    [SerializeField] private float health = 5f;
    [SerializeField] private float sightRange = 10f;
    [SerializeField] private float attackRange = 5f;

    private NavMeshAgent navMeshAgent;
    private Vector3 patrolTarget;
    private Transform player;
    private bool isWaiting;
    private bool isDead;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = patrolSpeed;
        navMeshAgent.stoppingDistance = 0.5f;
        navMeshAgent.updateRotation = false;
        player = GameObject.Find("Player").transform;  // Ensure player is named "PlayerObj"
    }

    void Start()
    {
        patrolTarget = PickPatrolPoint();
        navMeshAgent.SetDestination(patrolTarget);
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= sightRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (isWaiting) return;

        // Rotaciona para o próximo ponto de patrulha enquanto anda
        Vector3 moveDir = navMeshAgent.desiredVelocity;
        moveDir.y = 0f;
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(DwellAndShoot());
        }
    }


    private IEnumerator DwellAndShoot()
    {
        isWaiting = true;
        navMeshAgent.isStopped = true;

        // Wait before shooting
        yield return new WaitForSeconds(waitTimeAtPoint);



        // Pick the next patrol point and continue patrolling
        patrolTarget = PickPatrolPoint();
        navMeshAgent.SetDestination(patrolTarget);
        navMeshAgent.isStopped = false;
        isWaiting = false;
    }

    private Vector3 PickPatrolPoint()
    {
        Vector3 half = new Vector3(patrolAreaSize.x * 0.5f, 0, patrolAreaSize.z * 0.5f);
        Vector3 min = transform.position - half;
        Vector3 max = transform.position + half;
        Vector3 patrolPoint;
        do
        {
            float x = Random.Range(min.x, max.x);
            float z = Random.Range(min.z, max.z);
            patrolPoint = new Vector3(x, transform.position.y, z);
        }
        while (Vector3.Distance(transform.position, patrolPoint) < minPatrolDistance);
        return patrolPoint;
    }

    private void ChasePlayer()
    {
        navMeshAgent.SetDestination(player.position);

        // Rotaciona para a direção do movimento enquanto persegue o jogador
        Vector3 moveDir = navMeshAgent.desiredVelocity;
        moveDir.y = 0f;
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    }


    private void AttackPlayer()
    {
        // Rotaciona apenas no eixo Y
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0f;
        if (lookPos != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookPos);

        if (!isWaiting)
        {
            ShootAtPlayer();
            StartCoroutine(ShootCooldown());
        }
    }

    private void ShootAtPlayer()
    {
        // Calcula direção apenas no plano XZ (paralelo ao chão)
        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // ignora diferença de altura
        direction = direction.normalized;

        Vector3 spawnPos = transform.position + direction * 1.5f;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
    }


    private IEnumerator ShootCooldown()
    {
        isWaiting = true;
        yield return new WaitForSeconds(timeBetweenShots);
        isWaiting = false;
    }


    // Draw gizmos to help adjust in the Inspector
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
