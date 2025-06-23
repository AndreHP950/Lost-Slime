using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
public class HitFlash : MonoBehaviour
{
    [Header("Flash on Hit")]
    private Color hitColor = new Color(0.3f, 0f, 0f, 1f); // vermelho escuro
    [SerializeField] private float flashDuration = 0.2f;

    private Health health;
    // Lista de renderers deste objeto e filhos
    private List<Renderer> renderers = new();
    // Para cada renderer, armazena as cores originais de seus materiais (albedo)
    private List<Color[]> originalColors = new();
    // Para cada renderer, armazena as cores originais de emissao (se o material tiver)
    private List<Color[]> originalEmissionColors = new();

    /// <summary>
    /// Cor usada no próximo flash (pode ser alterada em runtime)
    /// </summary>
    public Color HitColor
    {
        get => hitColor;
        set => hitColor = value;
    }

    /// <summary>
    /// Duração do flash (pode ser alterada em runtime)
    /// </summary>
    public float FlashDuration
    {
        get => flashDuration;
        set => flashDuration = value;
    }

    void Awake()
    {
        health = GetComponent<Health>();

        // Coleta todos os renderers filhos (inclusive este)
        foreach (var rend in GetComponentsInChildren<Renderer>())
        {
            renderers.Add(rend);
            var mats = rend.materials;
            var cols = new Color[mats.Length];
            var emissCols = new Color[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                cols[i] = mats[i].color;
                // Se o material tiver emission, armazena sua cor original
                if (mats[i].HasProperty("_EmissionColor"))
                    emissCols[i] = mats[i].GetColor("_EmissionColor");
                else
                    emissCols[i] = Color.black;
            }
            originalColors.Add(cols);
            originalEmissionColors.Add(emissCols);
        }

        // Ao levar dano (onHit), dispara o Flash padrão
        health.onHit.AddListener(() => StartCoroutine(Flash()));
    }

    /// <summary>
    /// Força um flash imediatamente, usando os valores atuais de hitColor e flashDuration
    /// </summary>
    public void ForceFlash()
    {
        StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        // Aplica hitColor em todos os materiais e intensifica a emissao
        for (int r = 0; r < renderers.Count; r++)
        {
            var mats = renderers[r].materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_BaseColor"))
                    mats[i].SetColor("_BaseColor", hitColor);
                if (mats[i].HasProperty("_Color"))
                    mats[i].SetColor("_Color", hitColor);

                if (mats[i].HasProperty("_EmissionColor"))
                {
                    mats[i].EnableKeyword("_EMISSION");
                    mats[i].SetColor("_EmissionColor", hitColor * 2f);
                }
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // Restaura cores originais
        for (int r = 0; r < renderers.Count; r++)
        {
            var mats = renderers[r].materials;
            var cols = originalColors[r];
            var emissCols = originalEmissionColors[r];
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_BaseColor"))
                    mats[i].SetColor("_BaseColor", cols[i]);
                if (mats[i].HasProperty("_Color"))
                    mats[i].SetColor("_Color", cols[i]);

                if (mats[i].HasProperty("_EmissionColor"))
                {
                    mats[i].SetColor("_EmissionColor", emissCols[i]);
                    // Opcional: desabilitar a emissao se não for necessário
                    // mats[i].DisableKeyword("_EMISSION");
                }
            }
        }
    }

}
