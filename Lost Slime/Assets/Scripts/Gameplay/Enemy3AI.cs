using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class Enemy3AI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private Vector3 patrolAreaSize = new Vector3(10f, 1f, 10f);
    [SerializeField] private float minPatrolDistance = 2f;
    [SerializeField] private float waitTimeAtPoint = 2f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 12f; // Mais rápido
    [SerializeField] private float shootCooldown = 3f; // Cooldown entre rajadas
    [SerializeField] private int bulletsPerShot = 3;   // 3 projéteis por vez
    [SerializeField] private float spreadAngle = 15f;  // Ângulo de abertura do raio

    [Header("Enemy Stats")]
    [SerializeField] private float health = 10f;
    [SerializeField] private float sightRange = 10f;
    [SerializeField] private float attackRange = 5f;

    private NavMeshAgent navMeshAgent;
    private Vector3 patrolTarget;
    private Transform player;
    private bool isWaiting;
    private bool isDead;
    private float shootTimer = 0f;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = patrolSpeed;
        navMeshAgent.stoppingDistance = 0.5f;
        navMeshAgent.updateRotation = false;
        player = GameObject.Find("Player").transform;
    }

    void Start()
    {
        patrolTarget = PickPatrolPoint();
        navMeshAgent.SetDestination(patrolTarget);
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime;

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

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(DwellAndShoot());
        }
    }

    private IEnumerator DwellAndShoot()
    {
        isWaiting = true;
        navMeshAgent.isStopped = true;

        yield return new WaitForSeconds(waitTimeAtPoint);

        ShootAtPlayer();

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
    }

    private void AttackPlayer()
    {
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0f;
        if (lookPos != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookPos);

        if (shootTimer <= 0f)
        {
            ShootAtPlayer();
            shootTimer = shootCooldown;
        }
    }

    private void ShootAtPlayer()
    {
        // Direção base (plano XZ)
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        direction = direction.normalized;

        float startAngle = -spreadAngle * 0.5f;
        float angleStep = bulletsPerShot > 1 ? spreadAngle / (bulletsPerShot - 1) : 0f;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * direction;
            Vector3 spawnPos = transform.position + dir * 1.5f;

            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = dir * bulletSpeed;
            }
        }
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
