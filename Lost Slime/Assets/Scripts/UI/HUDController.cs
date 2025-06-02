using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class HUDController : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image[] healthSegments;

    [Header("Skills")]
    [SerializeField] private Image dashCooldownOverlay;
    [SerializeField] private Image liquifyCooldownOverlay;
    [SerializeField] private Image rCooldownOverlay;

    [Header("Referência do Player")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Vignette Dinâmico")]
    [SerializeField] private Image vignetteImage;      // Sprite: vignette (preto)
    [SerializeField] private Image redVignetteImage;   // Sprite: redVignette (vermelho)
    [SerializeField] private float vignettePulseDuration = 0.5f;
    [SerializeField] private float vignettePulseAlpha = 0.5f;
    [SerializeField] private float vignetteLowHealthAlpha = 0.7f;
    [SerializeField] private float vignetteLowHealthSpeed = 5f;

    [Header("Segmento perdido")]
    [SerializeField] private Color lostColor = Color.red;
    [SerializeField] private float lostDuration = 0.25f;
    [SerializeField] private float lostShake = 2f;

    private int lastHealth = -1;
    private Coroutine vignettePulseRoutine;
    private bool isLowHealthPulsing = false;

    void Start()
    {
        AutoAssignReferences();

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateHealthBar);
            playerHealth.onHit.AddListener(OnPlayerHit);
            UpdateHealthBar(playerHealth.Current);
            lastHealth = playerHealth.Current;
        }

        ResetVignettes();
    }

    void OnEnable()
    {
        ResetVignettes();
    }

    private void AutoAssignReferences()
    {
        // Player
        if (playerHealth == null)
            playerHealth = FindObjectOfType<Health>();
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();

        // Vignettes
        if (vignetteImage == null)
            vignetteImage = GameObject.Find("vignette")?.GetComponent<Image>();
        if (redVignetteImage == null)
            redVignetteImage = GameObject.Find("redVignette")?.GetComponent<Image>();

        // Overlays (busca por nome, tag ou ordem na hierarquia)
        if (dashCooldownOverlay == null)
            dashCooldownOverlay = GameObject.Find("CooldownOverlay")?.GetComponent<Image>();
        if (liquifyCooldownOverlay == null)
            liquifyCooldownOverlay = GameObject.Find("CooldownOverlay")?.GetComponent<Image>();
        if (rCooldownOverlay == null)
            rCooldownOverlay = GameObject.Find("CooldownOverlay")?.GetComponent<Image>();

        // Health Segments (busca todos filhos com Image e nome começando com "vida")
        if (healthSegments == null || healthSegments.Length == 0)
        {
            var allImages = GetComponentsInChildren<Image>(true);
            healthSegments = allImages
                .Where(img => img.name.ToLower().StartsWith("vida"))
                .OrderBy(img => img.name)
                .ToArray();
        }
    }

    private void ResetVignettes()
    {
        if (vignettePulseRoutine != null) StopCoroutine(vignettePulseRoutine);
        vignettePulseRoutine = null;
        isLowHealthPulsing = false;
        if (vignetteImage != null) vignetteImage.color = new Color(1, 1, 1, 0);
        if (redVignetteImage != null) redVignetteImage.color = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        if (playerMovement != null)
        {
            if (dashCooldownOverlay != null)
                dashCooldownOverlay.fillAmount = Mathf.Clamp01(playerMovement.dashCooldownTimer / playerMovement.dashCooldown);
            if (liquifyCooldownOverlay != null)
                liquifyCooldownOverlay.fillAmount = Mathf.Clamp01(playerMovement.liquidCooldownTimer / playerMovement.liquidCooldown);
            if (rCooldownOverlay != null)
                rCooldownOverlay.fillAmount = 0f;
        }

        if (playerHealth != null)
        {
            // Efeito de 1 de vida: só redVignette ativa e pulsando
            if (playerHealth.Current == 1)
            {
                if (!isLowHealthPulsing && redVignetteImage != null)
                {
                    if (vignettePulseRoutine != null) StopCoroutine(vignettePulseRoutine);
                    if (vignetteImage != null) vignetteImage.color = new Color(1, 1, 1, 0);
                    vignettePulseRoutine = StartCoroutine(LowHealthRedVignettePulse());
                    isLowHealthPulsing = true;
                }
            }
            else
            {
                if (isLowHealthPulsing && redVignetteImage != null)
                {
                    if (vignettePulseRoutine != null) StopCoroutine(vignettePulseRoutine);
                    redVignetteImage.color = new Color(1, 1, 1, 0);
                    isLowHealthPulsing = false;
                }
            }
        }
    }

    void UpdateHealthBar(int current)
    {
        if (healthSegments == null) return;

        if (lastHealth > current)
        {
            for (int i = current; i < lastHealth; i++)
            {
                if (i >= 0 && i < healthSegments.Length)
                    StartCoroutine(LoseSegmentEffect(healthSegments[i]));
            }
        }
        for (int i = 0; i < healthSegments.Length; i++)
            healthSegments[i].enabled = (i < current);

        lastHealth = current;
    }

    private IEnumerator LoseSegmentEffect(Image segment)
    {
        if (segment == null) yield break;
        float elapsed = 0f;
        Color original = segment.color;
        Vector3 originalPos = segment.rectTransform.anchoredPosition;

        while (elapsed < lostDuration)
        {
            segment.color = Color.Lerp(original, lostColor, Mathf.PingPong(elapsed * 8f, 1f));
            segment.rectTransform.anchoredPosition = originalPos + (Vector3)(Random.insideUnitCircle * lostShake);
            elapsed += Time.deltaTime;
            yield return null;
        }
        segment.color = original;
        segment.rectTransform.anchoredPosition = originalPos;
        segment.enabled = false;
    }

    private void OnPlayerHit()
    {
        if (playerHealth != null && playerHealth.Current > 1 && vignetteImage != null)
        {
            if (vignettePulseRoutine != null) StopCoroutine(vignettePulseRoutine);
            vignettePulseRoutine = StartCoroutine(VignettePulseOnce());
        }
    }

    private IEnumerator VignettePulseOnce()
    {
        float t = 0f;
        while (t < vignettePulseDuration)
        {
            float alpha = Mathf.Lerp(vignettePulseAlpha, 0, t / vignettePulseDuration);
            vignetteImage.color = new Color(1, 1, 1, alpha);
            t += Time.deltaTime;
            yield return null;
        }
        vignetteImage.color = new Color(1, 1, 1, 0);
    }

    private IEnumerator LowHealthRedVignettePulse()
    {
        while (true)
        {
            float pulse = (Mathf.Sin(Time.time * vignetteLowHealthSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(0, vignetteLowHealthAlpha, pulse);
            redVignetteImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }
}

