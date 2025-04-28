using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
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

    [Header("Ataque ao chegar")]
    [Tooltip("Prefab da bala")]
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("Velocidade das balas")]
    [SerializeField] private float bulletSpeed = 5f;
    [Tooltip("Distância do centro do inimigo onde as balas nascem")]
    [SerializeField] private float spawnOffset = 0.5f;
    [Tooltip("Quantas balas em círculo (ex.: 8 = 360°/8 a cada tiro)")]
    [SerializeField] private int bulletCount = 8;

    // componentes e estado
    private Rigidbody rb;
    private Vector3 patrolTarget;
    private Vector3 houseCenter;
    private bool isWaiting;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        // centro da casa = posição inicial
        houseCenter = transform.position;
    }

    void Start()
    {
        patrolTarget = PickPatrolPoint();
    }

    void FixedUpdate()
    {
        if (isWaiting) return;

        // anda rumo ao ponto
        Vector3 toTarget = patrolTarget - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude > 0.5f)
        {
            Vector3 dir = toTarget.normalized;
            rb.MovePosition(rb.position + dir * patrolSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // chegou: espera e dispara
            StartCoroutine(DwellAndShoot());
        }
    }

    private IEnumerator DwellAndShoot()
    {
        isWaiting = true;
        // 1) espera antes de atirar (opcional)
        yield return new WaitForSeconds(waitTimeAtPoint * 0.5f);

        // 2) dispara em círculo
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = 360f / bulletCount * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 spawnPos = transform.position + dir * spawnOffset;

            var proj = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));
            // ignora colisão com este inimigo
            var colProj = proj.GetComponent<Collider>();
            var colSelf = GetComponent<Collider>();
            if (colProj != null && colSelf != null)
                Physics.IgnoreCollision(colProj, colSelf);

            // configura velocidade
            var rbProj = proj.GetComponent<Rigidbody>();
            if (rbProj != null)
            {
                rbProj.useGravity = false;
                rbProj.linearVelocity = dir * bulletSpeed;
            }
        }

        // 3) espera o restante do tempo
        yield return new WaitForSeconds(waitTimeAtPoint * 0.5f);

        // 4) escolhe próximo ponto e segue patrulha
        patrolTarget = PickPatrolPoint();
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

    // Desenha gizmos para ajudar a ajustar no Inspector
    void OnDrawGizmosSelected()
    {
        // casa
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(houseCenter, houseSize);
        // ponto de destino
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(patrolTarget, 0.2f);
        }
    }
}
