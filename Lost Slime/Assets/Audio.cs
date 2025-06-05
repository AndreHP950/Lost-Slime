using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Settings")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float pitchVariation = 0.1f; // Variação de ±0.1 no pitch

    [Header("SFX Clips")]
    [SerializeField] private AudioClip playButtonClip;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip healClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip deathSongClip;
    [SerializeField] private AudioClip slimeWalkClip;

    void Awake()
    {
        // Implementa o Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Se nenhum AudioSource for atribuído no Inspector, adiciona um componente
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Toca um efeito sonoro com variação randomica de pitch.
    /// </summary>
    /// <param name="clip">Áudio a ser tocado.</param>
    /// <param name="volume">Volume do áudio (padrão é 1f).</param>
    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(clip, volume);
    }

    // Métodos auxiliares para facilitar chamadas em outros scripts:

    public void PlayButton() => PlaySfx(playButtonClip);
    public void PlayDamage() => PlaySfx(damageClip);
    public void PlayHeal() => PlaySfx(healClip);
    public void PlayDeath() => PlaySfx(deathClip);
    public void PlayDeathSong() => PlaySfx(deathSongClip);

    public void PlaySlimeWalk() => PlaySfx(slimeWalkClip);

}
