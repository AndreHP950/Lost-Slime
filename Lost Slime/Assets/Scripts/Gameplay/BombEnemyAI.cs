using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent), typeof(SphereCollider), typeof(Health))]
public class BombEnemyAI : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float explosionRange = 2f;
    [SerializeField] private float explosionDelay = 3f;
    [SerializeField] private int explosionDamage = 2;
    [SerializeField] private Color blinkColor = Color.white;

    private NavMeshAgent agent;
    private Transform player;
    private bool isChasing = false;
    private bool isExploding = false;

    // Alterado para armazenar todos renderers e cores originais
    private List<Renderer> renderers = new();
    private List<Color[]> originalColors = new();

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        agent.updateRotation = false;
        player = GameObject.Find("Player").transform;

        // Coleta todos os renderers filhos (inclusive este),
        // instancia os materiais para evitar alterar os compartilhados
        foreach (var rend in GetComponentsInChildren<Renderer>())
        {
            Material[] mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(mats[i]);
            }
            rend.materials = mats;
            renderers.Add(rend);
            Color[] cols = new Color[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                cols[i] = mats[i].color;
            }
            originalColors.Add(cols);
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
            agent.SetDestination(transform.position); // parado
        }
    }

    private IEnumerator ChaseAndExplode()
    {
        isChasing = true;
        agent.speed = chaseSpeed;
        float timer = 0f;
        float blinkInterval = 1f; // começa em 1 segundo

        while (timer < explosionDelay)
        {
            // Anda reto até o player (mantendo a mesma altura)
            Vector3 target = player.position;
            target.y = transform.position.y;
            agent.SetDestination(target);

            // Pisca (efeito similar ao HitFlash)
            SetColor(blinkColor);
            yield return new WaitForSeconds(blinkInterval * 0.5f);
            RestoreColor();
            yield return new WaitForSeconds(blinkInterval * 0.5f);

            timer += blinkInterval;
            blinkInterval = Mathf.Max(blinkInterval / 2f, 0.05f); // nunca menor que 0.05s
        }

        // Explode
        isExploding = true;
        agent.isStopped = true;
        Explode();
    }

    private void Explode()
    {
        // Dano em área
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
        // Aqui você pode adicionar efeito visual ou partículas
        Destroy(gameObject);
    }

    // Altera a cor de todos os materiais dos renderers para a cor desejada
    private void SetColor(Color color)
    {
        foreach (var renderer in renderers)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                SetMaterialColor(mats[i], color);
            }
        }
    }

    // Restaura as cores originais de todos os materiais dos renderers
    private void RestoreColor()
    {
        for (int r = 0; r < renderers.Count; r++)
        {
            Material[] mats = renderers[r].materials;
            Color[] cols = originalColors[r];
            for (int i = 0; i < mats.Length; i++)
            {
                SetMaterialColor(mats[i], cols[i]);
            }
        }
    }

    // Suporte para URP/Lit e Standard/Unlit
    private void SetMaterialColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color); // URP/Lit
        else if (mat.HasProperty("_Color"))
            mat.color = color; // Standard/Unlit
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
