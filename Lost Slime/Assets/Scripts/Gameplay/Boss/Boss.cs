using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BossAI : MonoBehaviour
{
    public enum BossMode { Mode1, Mode2, Mode3 }

    [Header("Geral")]
    [SerializeField] private Transform player;
    [SerializeField] private float switchModeMinTime = 4f;
    [SerializeField] private float switchModeMaxTime = 7f;

    [Header("Modo 1 (Patrulha Triângulo)")]
    [SerializeField] private float mode1Speed = 8f;
    [SerializeField] private Vector3 mode1Area = new Vector3(30, 1, 30);
    [SerializeField] private float mode1MinDist = 5f;
    [SerializeField] private float mode1Wait = 1.5f;
    [SerializeField] private float mode1Timeout = 5f;
    [SerializeField] private GameObject mode1BulletPrefab;
    [SerializeField] private float mode1BulletSpeed = 8f;
    [SerializeField] private int mode1CircleCount = 8;
    [SerializeField] private float mode1SpawnOffset = 2f;

    [Header("Modo 2 (Rajada Frontal Lenta)")]
    [SerializeField] private GameObject mode2BulletPrefab;
    [SerializeField] private float mode2BulletSpeed = 3f;
    [SerializeField] private int mode2BulletCount = 10;
    [SerializeField] private float mode2ShootInterval = 0.12f;

    [Header("Modo 3 (Cone Rajada)")]
    [SerializeField] private GameObject mode3BulletPrefab;
    [SerializeField] private float mode3BulletSpeed = 12f;
    [SerializeField] private int mode3BulletsPerShot = 3;
    [SerializeField] private float mode3SpreadAngle = 15f;
    [SerializeField] private float mode3ShootCooldown = 0.5f;
    [SerializeField] private int mode3BurstCount = 3;

    [Header("Tiro")]
    [Tooltip("Altura acima do pivô para spawn dos projéteis")]
    [SerializeField] private float bulletSpawnHeight = 2f;

    [Header("Efeito Slow")]
    [Tooltip("Multiplicador de velocidade quando atinge o player (0.5 = 50% da velocidade)")]
    [SerializeField] private float slowMultiplier = 0.5f;

    private NavMeshAgent agent;
    private BossMode currentMode;
    private bool isActive = false;
    private List<Vector3> trianglePoints;
    private Vector3 initialPosition;
    private bool playerDetected = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        isActive = true;
        initialPosition = transform.position;

        // Desativa o boss e a barra de vida no início
        gameObject.SetActive(false);

        // Desativa a barra de vida do boss (caso esteja ativa)
        GameObject bossHealthBar = GameObject.Find("BossHealthBar");
        if (bossHealthBar != null)
            bossHealthBar.SetActive(false);
    }


    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        StartCoroutine(PlayerDetectionRoutine());
    }

    IEnumerator PlayerDetectionRoutine()
    {
        while (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
            yield return null;
        }
        playerDetected = true;
        StartCoroutine(ModeSwitcher());
    }

    IEnumerator ModeSwitcher()
    {
        // Sequência: 1,2,1,3,1,2,1,3...
        while (isActive)
        {
            yield return StartCoroutine(Mode1Routine());
            yield return StartCoroutine(Mode2Routine());
            yield return StartCoroutine(Mode1Routine());
            yield return StartCoroutine(Mode3Routine());
        }
    }

    // MODO 1: Patrulha Triângulo e tiro circular, retorna ao ponto inicial
    IEnumerator Mode1Routine()
    {
        agent.speed = mode1Speed;
        trianglePoints = GenerateValidTrianglePoints();
        if (trianglePoints.Count == 0)
        {
            Debug.LogWarning("[BossAI] Nenhum ponto válido encontrado para o triângulo!");
            yield break;
        }

        int pointIndex = 0;
        foreach (var target in trianglePoints)
        {
            Debug.Log($"[BossAI] Indo para o ponto do triângulo {pointIndex + 1}: {target}");
            agent.SetDestination(target);
            float startTime = Time.time;
            bool shot = false;
            bool soundPlayed = false; // Para garantir que o som seja tocado apenas uma vez

            while (true)
            {
                float dist = Vector3.Distance(transform.position, target);
                if (!agent.pathPending && agent.pathStatus != NavMeshPathStatus.PathComplete)
                {
                    Debug.LogWarning("[BossAI] Caminho inválido para o ponto do triângulo, pulando para o próximo.");
                    break;
                }
                if (dist <= agent.stoppingDistance + 0.2f)
                {
                    if (!shot)
                    {
                        agent.isStopped = true;
                        Debug.Log($"[BossAI] Chegou ao ponto {pointIndex + 1}, atirando imediatamente.");

                        // Toca o som do modo 1 apenas uma vez
                        if (!soundPlayed && AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlayBossMode1Sound();
                            soundPlayed = true;
                        }

                        // Atira 3 vezes no ponto
                        for (int i = 0; i < 3; i++)
                        {
                            // Ajuste para disparar em espiral
                            float angleOffset = i * 15f;
                            for (int j = 0; j < mode1CircleCount; j++)
                            {
                                float angle = 360f / mode1CircleCount * j + angleOffset;
                                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                                Vector3 spawnPos = transform.position + dir * mode1SpawnOffset + Vector3.up * bulletSpawnHeight;
                                var proj = Instantiate(mode1BulletPrefab, spawnPos, Quaternion.LookRotation(dir));
                                if (proj == null)
                                {
                                    Debug.LogError("[BossAI] Falha ao instanciar o projétil do boss!");
                                    continue;
                                }
                                AddBulletSlowEffect(proj);
                                var rb = proj.GetComponent<Rigidbody>();
                                if (rb != null)
                                {
                                    rb.linearVelocity = dir * mode1BulletSpeed;
                                }
                                else
                                {
                                    Debug.LogWarning("[BossAI] Projétil instanciado não possui Rigidbody!");
                                }
                            }
                            Debug.Log($"[BossAI] Disparou {mode1CircleCount} projéteis em espiral no ponto {pointIndex + 1}.");
                            yield return new WaitForSeconds(0.5f);
                        }
                        shot = true;
                        float timer = 0f;
                        while (timer < 0.5f)
                        {
                            LookAtPlayer();
                            timer += Time.deltaTime;
                            yield return null;
                        }
                        agent.isStopped = false;
                        break;
                    }
                }
                if (Time.time - startTime > mode1Timeout)
                {
                    Debug.LogWarning("[BossAI] Timeout ao tentar chegar no ponto do triângulo, pulando para o próximo.");
                    break;
                }
                Vector3 dirMove = agent.desiredVelocity;
                dirMove.y = 0;
                if (dirMove.sqrMagnitude > 0.01f)
                {
                    Quaternion rot = Quaternion.LookRotation(dirMove);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8f);
                }
                LookAtPlayer();
                yield return null;
            }
            pointIndex++;
        }

        Debug.Log("[BossAI] Retornando ao ponto inicial.");
        agent.SetDestination(initialPosition);
        float returnStart = Time.time;
        while (true)
        {
            float dist = Vector3.Distance(transform.position, initialPosition);
            if (!agent.pathPending && agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                Debug.LogWarning("[BossAI] Caminho inválido para o ponto inicial.");
                break;
            }
            if (dist <= agent.stoppingDistance + 0.2f)
            {
                agent.isStopped = true;
                break;
            }
            if (Time.time - returnStart > mode1Timeout)
            {
                Debug.LogWarning("[BossAI] Timeout ao tentar retornar ao ponto inicial.");
                break;
            }
            Vector3 dir = agent.desiredVelocity;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8f);
            }
            LookAtPlayer();
            yield return null;
        }
        float waitTimer = 0f;
        while (waitTimer < 0.1f)
        {
            LookAtPlayer();
            waitTimer += Time.deltaTime;
            yield return null;
        }
        agent.isStopped = false;
    }

    IEnumerator Mode2Routine()
    {
        agent.isStopped = true;
        for (int i = 0; i < mode2BulletCount; i++)
        {
            LookAtPlayer();
            Vector3 dir = (player.position - transform.position);
            dir.y = 0;
            dir = dir.normalized;
            Vector3 spawnPos = transform.position + dir * 2.5f + Vector3.up * bulletSpawnHeight;
            var proj = Instantiate(mode2BulletPrefab, spawnPos, Quaternion.LookRotation(dir));
            AddBulletSlowEffect(proj);
            var rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = dir * mode2BulletSpeed;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayBossMode2Sound();
            float timer = 0f;
            while (timer < mode2ShootInterval)
            {
                LookAtPlayer();
                timer += Time.deltaTime;
                yield return null;
            }
        }
        agent.isStopped = false;
    }

    IEnumerator Mode3Routine()
    {
        agent.isStopped = true;
        bool soundPlayed = false; // Para tocar o som do modo 3 apenas uma vez
        for (int burst = 0; burst < mode3BurstCount; burst++)
        {
            LookAtPlayer();
            Vector3 dir = (player.position - transform.position);
            dir.y = 0;
            dir = dir.normalized;
            float startAngle = -mode3SpreadAngle * 0.5f;
            float angleStep = mode3BulletsPerShot > 1 ? mode3SpreadAngle / (mode3BulletsPerShot - 1) : 0f;
            if (!soundPlayed && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossMode3Sound();
                soundPlayed = true;
            }
            for (int i = 0; i < mode3BulletsPerShot; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector3 shotDir = Quaternion.Euler(0, angle, 0) * dir;
                Vector3 spawnPos = transform.position + shotDir * 2.5f + Vector3.up * bulletSpawnHeight;
                var proj = Instantiate(mode3BulletPrefab, spawnPos, Quaternion.LookRotation(shotDir));
                AddBulletSlowEffect(proj);
                var rb = proj.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = shotDir * mode3BulletSpeed;
            }
            float timer = 0f;
            while (timer < mode3ShootCooldown)
            {
                LookAtPlayer();
                timer += Time.deltaTime;
                yield return null;
            }
        }
        agent.isStopped = false;
    }

    // Adiciona o efeito de slow aos projéteis
    private void AddBulletSlowEffect(GameObject projectile)
    {
        if (projectile == null) return;
        BulletSlowEffect slowEffect = projectile.AddComponent<BulletSlowEffect>();
        slowEffect.slowMultiplier = slowMultiplier;
    }

    // Gera 3 pontos em triângulo dentro da área, retornando apenas os pontos válidos no NavMesh
    List<Vector3> GenerateValidTrianglePoints()
    {
        List<Vector3> validPoints = new List<Vector3>();
        Vector3 center = initialPosition;
        float radius = Mathf.Min(mode1Area.x, mode1Area.z) * 0.48f;
        NavMeshHit hit;
        for (int i = 0; i < 3; i++)
        {
            float angle = 120f * i * Mathf.Deg2Rad;
            Vector3 pt = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            if (NavMesh.SamplePosition(pt, out hit, 1.5f, NavMesh.AllAreas))
            {
                validPoints.Add(hit.position);
            }
        }
        return validPoints;
    }

    // Faz o boss olhar para o player (apenas no eixo Y)
    void LookAtPlayer()
    {
        if (player == null) return;
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0f;
        if (lookPos.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    }

    // GIZMOS para visualização da área de patrulha e triângulo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = Application.isPlaying ? initialPosition : transform.position;
        Gizmos.DrawWireCube(center, mode1Area);
        List<Vector3> tri = new List<Vector3>();
        float radius = Mathf.Min(mode1Area.x, mode1Area.z) * 0.48f;
        for (int i = 0; i < 3; i++)
        {
            float angle = 120f * i * Mathf.Deg2Rad;
            Vector3 pt = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            tri.Add(pt);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(pt, 1.0f);
        }
        Gizmos.color = Color.yellow;
        for (int i = 0; i < 3; i++)
        {
            Gizmos.DrawLine(tri[i], tri[(i + 1) % 3]);
        }
    }
}

// Classe para o efeito de slow nos projéteis
public class BulletSlowEffect : MonoBehaviour
{
    [HideInInspector] public float slowMultiplier = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement pm = collision.gameObject.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.ApplySpeedMultiplier(slowMultiplier);
                Debug.Log($"Jogador atingido por projétil do boss. Velocidade reduzida para {slowMultiplier * 100}% da velocidade normal.");
            }
        }
        Destroy(gameObject);
    }
}
