using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
public class HitFlash : MonoBehaviour
{
    [Header("Flash on Hit")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;

    private Health health;
    // Lista de renderers deste objeto e filhos
    private List<Renderer> renderers = new();
    // Para cada renderer, armazena as cores originais de seus materiais
    private List<Color[]> originalColors = new();

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
            for (int i = 0; i < mats.Length; i++)
                cols[i] = mats[i].color;
            originalColors.Add(cols);
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
        // Aplica hitColor em todos os materiais
        for (int r = 0; r < renderers.Count; r++)
        {
            var mats = renderers[r].materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i].color = hitColor;
        }

        yield return new WaitForSeconds(flashDuration);

        // Restaura cores originais
        for (int r = 0; r < renderers.Count; r++)
        {
            var mats = renderers[r].materials;
            var cols = originalColors[r];
            for (int i = 0; i < mats.Length; i++)
                mats[i].color = cols[i];
        }
    }
}
