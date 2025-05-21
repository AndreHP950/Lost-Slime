using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class Enemy2AI : MonoBehaviour
{
    [Header("Patrulha")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private Vector3 houseSize = new Vector3(10f, 1f, 10f);
    [SerializeField] private float minPatrolDistance = 2f;
    [SerializeField] private float waitTimeAtPoint = 2f;

    [Header("Ataque ao chegar")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private int bulletCount = 1;

    [Header("Detecção do Jogador")]
    [SerializeField] private float sightRange = 10f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float maxChaseDistance = 15f; // Max distance from house to chase
    [SerializeField] private LayerMask whatIsPlayer;

    // Components and state
    private NavMeshAgent agent;
    private Vector3 patrolTarget;
    private Vector3 houseCenter;
    private bool isWaiting;
    private Transform player;

    // State flags
    private bool playerInSightRange;
    private bool playerInAttackRange;
    private bool alreadyAttacked;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.5f;
        houseCenter = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Start()
    {
        patrolTarget = PickPatrolPoint();
        agent.SetDestination(patrolTarget);
    }

    void Update()
    {
        if (player == null)
        {
            Debug.Log($"{name}: Player not found!");
            return;
        }

        float playerDistanceFromHouse = Vector3.Distance(player.position, houseCenter);

        // Check for player in sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        Debug.Log($"{name}: playerInSightRange={playerInSightRange}, playerInAttackRange={playerInAttackRange}, playerDistanceFromHouse={playerDistanceFromHouse}");

        if (playerInSightRange && playerDistanceFromHouse <= maxChaseDistance)
        {
            if (playerInAttackRange)
            {
                Debug.Log($"{name}: Attacking player");
                AttackPlayer();
            }
            else
            {
                Debug.Log($"{name}: Chasing player");
                ChasePlayer();
            }
        }
        else if (playerDistanceFromHouse > maxChaseDistance)
        {
            Debug.Log($"{name}: Returning to house");
            ReturnToHouse();
        }
        else
        {
            Debug.Log($"{name}: Patrolling");
            Patrol();
        }
    }


    private void Patrol()
    {
        agent.speed = patrolSpeed;
        if (isWaiting) return;

        agent.SetDestination(patrolTarget);

        Vector3 toTarget = patrolTarget - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude <= agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(DwellAndShoot());
        }
    }

    private void ChasePlayer()
    {
        agent.speed = patrolSpeed * 1.5f;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;
        // Look at player horizontally only
        Vector3 lookAtPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookAtPos);

        if (!alreadyAttacked)
        {
            // Shoot at player
            Vector3 dir = (player.position - transform.position).normalized;
            Vector3 spawnPos = transform.position + dir * spawnOffset;

            var proj = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));
            var colProj = proj.GetComponent<Collider>();
            var colSelf = GetComponent<Collider>();
            if (colProj != null && colSelf != null)
                Physics.IgnoreCollision(colProj, colSelf);

            var rbProj = proj.GetComponent<Rigidbody>();
            if (rbProj != null)
            {
                rbProj.useGravity = false;
                rbProj.linearVelocity = dir * bulletSpeed;
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), waitTimeAtPoint);
        }
    }

    private void ReturnToHouse()
    {
        agent.speed = patrolSpeed * 1.2f;
        agent.isStopped = false;
        agent.SetDestination(houseCenter);
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private IEnumerator DwellAndShoot()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(waitTimeAtPoint * 0.5f);

        // Shoot at player if in sight, otherwise do nothing
        if (player != null && playerInSightRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            Vector3 spawnPos = transform.position + dir * spawnOffset;

            var proj = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));
            var colProj = proj.GetComponent<Collider>();
            var colSelf = GetComponent<Collider>();
            if (colProj != null && colSelf != null)
                Physics.IgnoreCollision(colProj, colSelf);

            var rbProj = proj.GetComponent<Rigidbody>();
            if (rbProj != null)
            {
                rbProj.useGravity = false;
                rbProj.linearVelocity = dir * bulletSpeed;
            }
        }

        yield return new WaitForSeconds(waitTimeAtPoint * 0.5f);

        patrolTarget = PickPatrolPoint();
        agent.SetDestination(patrolTarget);
        agent.isStopped = false;
        isWaiting = false;
    }

    private Vector3 PickPatrolPoint()
    {
        Vector3 half = new Vector3(houseSize.x * 0.5f, 0, houseSize.z * 0.5f);
        Vector3 min = houseCenter - half;
        Vector3 max = houseCenter + half;
        Vector3 pt;
        do
        {
            float x = Random.Range(min.x, max.x);
            float z = Random.Range(min.z, max.z);
            pt = new Vector3(x, transform.position.y, z);
        }
        while (Vector3.Distance(transform.position, pt) < minPatrolDistance);
        return pt;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(houseCenter, houseSize);
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(patrolTarget, 0.2f);
        }
    }
}
