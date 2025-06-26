using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.Cinemachine; // Certifique-se de ter o pacote instalado

[System.Serializable]
public class DropItem
{
    public GameObject dropPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.1f;
    public Vector3 offset = Vector3.zero;
}

[RequireComponent(typeof(Health))]
public class EnemyDeath : MonoBehaviour
{
    [Header("Drops Configuráveis")]
    [Tooltip("Itens a serem droppados com suas chances e offsets")]
    [SerializeField] private DropItem[] dropItems;
    [Tooltip("Espaçamento aleatório para os drops (para evitar sobreposição)")]
    [SerializeField] private float dropSpread = 0.5f;

    [Header("Extras de Morte (Opcional)")]
    [Tooltip("Marque para usar o efeito de morte completo (VFX, delay, shake, sinking). Geralmente usado para bosses.")]
    [SerializeField] private bool useDeathEffect = false;
    [Tooltip("Tempo total dos efeitos de morte (cooldown)")]
    [SerializeField] private float deathDelay = 3f;
    [Tooltip("Distância que o objeto será arrastado para baixo durante o efeito de morte")]
    [SerializeField] private float sinkDistance = 2f;
    [Tooltip("Prefab de efeito extra de morte (ex.: VFX especial)")]
    [SerializeField] private GameObject extraDeathEffectPrefab;
    [Tooltip("Duração do efeito extra de morte (após essa duração o VFX será destruído)")]
    [SerializeField] private float extraDeathEffectDuration = 3f;

    [Header("Camera Shake (Opcional)")]
    [SerializeField] private CinemachineCamera deathVirtualCamera; // Sua CinemachineCamera
    [SerializeField] private float shakeAmplitude = 3f;
    [SerializeField] private float shakeDuration = 0.5f;

    [Header("Som de Morte")]
    [Tooltip("Som de morte para tocar (opcional)")]
    [SerializeField] private AudioClip deathSound;

    void Awake()
    {
        GetComponent<Health>().onDied.AddListener(OnEnemyDied);
    }

    private void OnEnemyDied()
    {
        // Armazena a posição atual para os drops.
        Vector3 dropSpawnPosition = transform.position;

        // Desativa a movimentação e a física do inimigo.
        DisableMovement();

        // Troca os itens de drop na posição atual (antes dos efeitos de sinking).
        TryDropItems(dropSpawnPosition);

        // Toca o som de morte, se definido.
        if (deathSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(deathSound);

        // Se usar efeito de morte extra, execute a sequência; senão, destrói o objeto imediatamente.
        if (useDeathEffect)
        {
            StartCoroutine(ExtraDeathEffectsAndDestruction());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Desativa a movimentação/IA e a física para evitar que o objeto continue se movendo após a morte.
    /// </summary>
    private void DisableMovement()
    {
        var enemyAI = GetComponent<MonoBehaviour>(); // Substitua pelo seu script de IA, se aplicável.
        if (enemyAI != null)
            enemyAI.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    /// <summary>
    /// Itera sobre os itens dropáveis e instancia cada um usando sua chance, offset e um espalhamento aleatório.
    /// </summary>
    private void TryDropItems(Vector3 spawnPosition)
    {
        if (dropItems == null || dropItems.Length == 0)
            return;

        foreach (DropItem item in dropItems)
        {
            if (item.dropPrefab == null)
                continue;

            if (Random.value <= item.dropChance)
            {
                // Adiciona um valor aleatório para evitar sobreposição
                Vector2 randomCircle = Random.insideUnitCircle * dropSpread;
                Vector3 randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
                Vector3 finalPosition = spawnPosition + item.offset + randomOffset;
                Instantiate(item.dropPrefab, finalPosition, Quaternion.identity);
                Debug.Log("Drop: " + item.dropPrefab.name);
            }
        }
    }

    /// <summary>
    /// Executa os efeitos extras de morte (VFX, camera shake, sinking) e destrói o objeto após o deathDelay.
    /// </summary>
    private IEnumerator ExtraDeathEffectsAndDestruction()
    {
        // Instancia o efeito extra de morte (VFX), se configurado, e destrói-o após o tempo definido.
        if (extraDeathEffectPrefab != null)
        {
            GameObject extraEffect = Instantiate(extraDeathEffectPrefab, transform.position, transform.rotation);
            Destroy(extraEffect, extraDeathEffectDuration);
        }

        // Executa o efeito de shake na câmera em paralelo, se configurado.
        if (deathVirtualCamera != null && shakeAmplitude > 0f && shakeDuration > 0f)
        {
            StartCoroutine(ShakeCamera());
        }

        // Executa o efeito de afundamento (sinking) em paralelo, se o uso dele estiver habilitado.
        if (useDeathEffect && sinkDistance > 0f)
        {
            StartCoroutine(DragDown(deathDelay));
        }

        // Aguarda o tempo total dos efeitos antes de destruir o objeto.
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }

    private IEnumerator DragDown(float duration)
    {
        Vector3 initialPos = transform.position;
        Vector3 targetPos = initialPos - Vector3.up * sinkDistance;
        float t = 0f;
        while (t < duration)
        {
            transform.position = Vector3.Lerp(initialPos, targetPos, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
    }

    private IEnumerator ShakeCamera()
    {
        CinemachineBasicMultiChannelPerlin noise = deathVirtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise != null)
        {
            float originalAmplitude = noise.AmplitudeGain;
            float originalFrequency = noise.FrequencyGain;

            noise.AmplitudeGain = shakeAmplitude;
            noise.FrequencyGain = 20f; // Tremor brusco

            yield return new WaitForSeconds(shakeDuration);

            noise.AmplitudeGain = originalAmplitude;
            noise.FrequencyGain = originalFrequency;
        }
        else
        {
            yield return null;
        }
    }
}
