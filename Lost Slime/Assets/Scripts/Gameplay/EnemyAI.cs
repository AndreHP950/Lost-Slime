using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class EnemyPatrolShoot : MonoBehaviour
{
    [Header("Patrulha")]
    [Tooltip("Velocidade de patrulha")]
    [SerializeField] private float patrolSpeed = 3f;
    [Tooltip("Tamanho da casa de patrulha (X,Y,Z)")]
    [SerializeField] private Vector3 houseSize = new Vector3(10f, 1f, 10f);
    [Tooltip("Distância mínima entre pontos gerados")]
    [SerializeField] private float minPatrolDistance = 2f;
    [Tooltip("Tempo que ele espera ao chegar no ponto antes de atirar e seguir")]
    [SerializeField] private float waitTimeAtPoint = 2f;
    [Tooltip("Tempo máximo tentando chegar no ponto de patrulha (segundos)")]
    [SerializeField] private float maxTimeToReachPoint = 5f;

    [Header("Ataque ao chegar")]
    [Tooltip("Prefab da bala")]
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("Velocidade das balas")]
    [SerializeField] private float bulletSpeed = 5f;
    [Tooltip("Distância do centro do inimigo onde as balas nascem")]
    [SerializeField] private float spawnOffset = 0.5f;
    [Tooltip("Quantas balas em círculo (ex.: 8 = 360°/8 a cada tiro)")]
    [SerializeField] private int bulletCount = 8;

    // Components and state
    private NavMeshAgent navMeshAgent;
    private Vector3 patrolTarget;
    private Vector3 houseCenter;
    private bool isWaiting;
    private float patrolStartTime;
    private Coroutine patrolTimeoutRoutine;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = patrolSpeed;
        navMeshAgent.stoppingDistance = 0.5f;

        // Center of the patrol area is the initial position
        houseCenter = transform.position;
    }

    void Start()
    {
        SetNewPatrolTarget();
    }

    void Update()
    {
        if (isWaiting) return;

        // Timeout: se passou do tempo máximo, escolhe outro ponto
        if (Time.time - patrolStartTime > maxTimeToReachPoint)
        {

            SetNewPatrolTarget();
            return;
        }

        // Check if the agent has reached the destination
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
            {
                StartCoroutine(DwellAndShoot());
            }
        }
    }

    private void SetNewPatrolTarget()
    {
        patrolTarget = PickPatrolPoint();
        navMeshAgent.SetDestination(patrolTarget);
        patrolStartTime = Time.time;
    }

    private IEnumerator DwellAndShoot()
    {
        isWaiting = true;
        navMeshAgent.isStopped = true;

        yield return new WaitForSeconds(waitTimeAtPoint * 0.5f);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = 360f / bulletCount * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
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

        navMeshAgent.isStopped = false;
        isWaiting = false;
        SetNewPatrolTarget();
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(houseCenter, houseSize);
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(patrolTarget, 0.2f);
        }
    }
}
