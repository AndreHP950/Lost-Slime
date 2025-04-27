using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack, Search }
    private State currentState;

    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float checkInterval = 0.5f;
    [SerializeField] private float dwellTime = 1f;

    [Header("Visão e Detecção")]
    [Tooltip("Raio 360° curto")]
    [SerializeField] private float peripheralRadius = 4f;
    [Tooltip("Raio frontal maior")]
    [SerializeField] private float viewRadius = 12f;
    [SerializeField, Range(0, 360f)] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Casa (Patrulha)")]
    [Tooltip("Tamanho da área de patrulha (X,Z)")]
    [SerializeField] private Vector3 houseSize = new Vector3(10f, 1f, 10f);
    [SerializeField] private float edgeBuffer = 1f;
    [SerializeField] private float minPatrolDist = 5f;
    [Tooltip("Distância máxima antes de abandonar chase")]
    [SerializeField] private float returnRange = 15f;

    [Header("Ataque")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float spawnOffset = 1f;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float shootRange = 6f;

    // componentes e estado interno
    private Rigidbody rb;
    private Transform player;
    private Vector3 houseCenter;
    private Vector3 patrolTarget;
    private Vector3 lastKnownPosition;
    private float checkTimer;
    private bool isDwelling;
    private float dwellTimer;
    private float shootTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        player = GameObject.FindWithTag("Player").transform;
        houseCenter = transform.position;
    }

    void Start()
    {
        SetState(State.Patrol);
        patrolTarget = PickPatrolPoint();
    }

    void Update()
    {
        // apenas decrementa timer de detecção e dispara CheckForPlayer
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            CheckForPlayer();
            checkTimer = checkInterval;
        }

        // timer de tiro
        shootTimer = Mathf.Max(0f, shootTimer - Time.deltaTime);
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Patrol: PatrolBehaviour(); break;
            case State.Chase: ChaseBehaviour(); break;
            case State.Attack: AttackBehaviour(); break;
            case State.Search: SearchBehaviour(); break;
        }
    }

    private void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"State: {newState}");
    }

    // ─── Patrulha ──────────────────────────
    private void PatrolBehaviour()
    {
        if (isDwelling) return;

        // se bater num obstáculo a 1m à frente, já escolhe outro ponto
        var dirToT = (patrolTarget - transform.position).normalized;
        if (Physics.Raycast(transform.position, dirToT, 1f, obstacleMask))
        {
            patrolTarget = PickPatrolPoint();
            return;
        }

        MoveTo(patrolTarget);
        if (Vector3.Distance(transform.position, patrolTarget) < 0.5f)
            StartCoroutine(DwellAndNext());
    }

    private IEnumerator DwellAndNext()
    {
        isDwelling = true;
        dwellTimer = dwellTime;
        while (dwellTimer > 0f)
        {
            dwellTimer -= Time.deltaTime;
            yield return null;
        }
        patrolTarget = PickPatrolPoint();
        isDwelling = false;
    }

    // ─── Perseguir até entrar em shootRange ───────────
    private void ChaseBehaviour()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        // se dentro do alcance de tiro, troca para Attack
        if (dist <= shootRange)
        {
            SetState(State.Attack);
            return;
        }

        MoveTo(player.position);

        // se escapar demais da área da casa, parte pra Search
        if (Vector3.Distance(transform.position, houseCenter) > returnRange)
            SetState(State.Search);
    }

    // ─── Atirar ─────────────────────────────────────────
    private void AttackBehaviour()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        // se fugir do alcance de tiro, volta a persegue
        if (dist > shootRange)
        {
            SetState(State.Chase);
            return;
        }

        // rotaciona para mirar no player
        var dir = (player.position - transform.position);
        dir.y = 0f;
        transform.rotation = Quaternion.LookRotation(dir.normalized);

        // atira quando o timer zerar
        if (shootTimer == 0f)
        {
            Debug.Log("Enemy: Shoot");
            Vector3 spawnPos = transform.position + dir.normalized * spawnOffset;
            var b = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));
            var bRb = b.GetComponent<Rigidbody>();
            bRb.linearVelocity = dir.normalized * bulletSpeed;

            shootTimer = 1f / fireRate;
        }
    }

    // ─── Buscar última posição e retornar ───────────────
    private void SearchBehaviour()
    {
        MoveTo(lastKnownPosition);
        if (Vector3.Distance(transform.position, lastKnownPosition) < 0.5f)
            SetState(State.Patrol);
    }

    // ─── Movimento comum ────────────────────────────────
    private void MoveTo(Vector3 target)
    {
        var dir = (target - transform.position).normalized;
        var nextPos = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);
    }

    // ─── Detecção do Player ─────────────────────────────
    private void CheckForPlayer()
    {
        var toP = player.position - transform.position;
        toP.y = 0f;
        float dist = toP.magnitude;
        var dir = toP.normalized;

        // 1) Detecção 360° curta
        if (dist <= peripheralRadius)
        {
            SpotPlayer();
            return;
        }

        // 2) Detecção em cone frontal
        if (dist <= viewRadius)
        {
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle <= viewAngle * 0.5f)
            {
                // só “vê” se não houver obstáculo entre os dois
                if (!Physics.Raycast(transform.position, dir, dist, obstacleMask))
                {
                    SpotPlayer();
                }
            }
        }
    }

    private void SpotPlayer()
    {
        lastKnownPosition = player.position;
        SetState(State.Chase);
        Debug.Log("Enemy: Player spotted!");
    }

    // ─── Sorteia novo ponto dentro da “casa” ───────────
    private Vector3 PickPatrolPoint()
    {
        Vector3 half = new Vector3(houseSize.x * 0.5f, 0, houseSize.z * 0.5f);
        Vector3 min = houseCenter - half + new Vector3(edgeBuffer, 0, edgeBuffer);
        Vector3 max = houseCenter + half - new Vector3(edgeBuffer, 0, edgeBuffer);
        Vector3 pt;
        do
        {
            float x = Random.Range(min.x, max.x);
            float z = Random.Range(min.z, max.z);
            pt = new Vector3(x, transform.position.y, z);
        }
        while (Vector3.Distance(transform.position, pt) < minPatrolDist);
        Debug.Log($"PickPatrol: {pt}");
        return pt;
    }
}
