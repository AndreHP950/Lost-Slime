using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Settings")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float pitchVariation = 0.1f; // Varia��o de �0.1 no pitch

    [Header("SFX Clips")]
    [SerializeField] private AudioClip playButtonClip;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip healClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip deathSongClip;
    [SerializeField] private AudioClip slimeWalkClip;
    [SerializeField] private AudioClip hitClip;

    [Header("Boss SFX")]
    [SerializeField] private AudioClip bossMode1SFX; // Som completo para o modo 1 (c�rculo de proj�teis)
    [SerializeField] private AudioClip bossMode2SFX; // Som individual para cada tiro do modo 2
    [SerializeField] private AudioClip bossMode3SFX; // Som completo para o modo 3 (cone de tiros)

    [Header("M�sica de Fundo")]
    [SerializeField] private AudioSource musicSource;           // AudioSource para m�sica de fundo
    [SerializeField] private AudioClip[] backgroundMusicClips;    // Array com os clipes de trilha sonora

    private int[] trackOrder;
    private int currentTrackIndex = 0;

    void Awake()
    {
        // Implementa o Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Se nenhum AudioSource for atribu�do no Inspector, adiciona um componente para SFX
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        // Verifica e adiciona o AudioSource para m�sica, se necess�rio
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = false;  // Usamos sem loop para controlar a sequ�ncia
        }

        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        SetMusicVolume(musicVol);
        SetSfxVolume(sfxVol);
    }

    void Start()
    {
        // Inicia a trilha sonora se houver clipes configurados
        if (backgroundMusicClips != null && backgroundMusicClips.Length > 0)
        {
            InitializeMusicOrder();
            PlayNextTrack();
        }
    }

    /// <summary>
    /// Embaralha os �ndices dos clipes para criar uma sequ�ncia aleat�ria.
    /// </summary>
    /// 
    public float GetMusicVolume() => musicSource != null ? musicSource.volume : 1f;
    public float GetSfxVolume() => sfxSource != null ? sfxSource.volume : 1f;

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSfxVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }


    private void InitializeMusicOrder()
    {
        int count = backgroundMusicClips.Length;
        trackOrder = new int[count];
        for (int i = 0; i < count; i++)
            trackOrder[i] = i;
        // Embaralha (algoritmo de Fisher-Yates)
        for (int i = count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            int temp = trackOrder[i];
            trackOrder[i] = trackOrder[rand];
            trackOrder[rand] = temp;
        }
        currentTrackIndex = 0;
    }

    /// <summary>
    /// Reproduz o pr�ximo clipe na ordem embaralhada.
    /// </summary>
    private void PlayNextTrack()
    {
        if (backgroundMusicClips.Length == 0) return;

        int trackIndex = trackOrder[currentTrackIndex];
        musicSource.clip = backgroundMusicClips[trackIndex];
        musicSource.Play();

        // Atualiza o �ndice para o pr�ximo clipe
        currentTrackIndex = (currentTrackIndex + 1) % backgroundMusicClips.Length;
        // Se voltarmos ao in�cio, podemos reembaralhar a ordem, se quiser uma nova sequ�ncia
        if (currentTrackIndex == 0)
        {
            InitializeMusicOrder();
        }

        // Come�a uma coroutine para monitorar o fim do clipe
        StartCoroutine(WaitForMusicToEnd());
    }

    private System.Collections.IEnumerator WaitForMusicToEnd()
    {
        while (musicSource.isPlaying)
        {
            yield return null;
        }
        PlayNextTrack();
    }

    /// <summary>
    /// Toca um efeito sonoro com varia��o randomica de pitch.
    /// </summary>
    /// <param name="clip">�udio a ser tocado.</param>
    /// <param name="volume">Volume do �udio (padr�o � 1f).</param>
    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(clip, volume);
    }

    // M�todos auxiliares para facilitar chamadas em outros scripts:
    public void PlayButton() => PlaySfx(playButtonClip);
    public void PlayDamage() => PlaySfx(damageClip);
    public void PlayHeal() => PlaySfx(healClip);
    public void PlayDeath() => PlaySfx(deathClip);
    public void PlayDeathSong() => PlaySfx(deathSongClip);
    public void PlaySlimeWalk() => PlaySfx(slimeWalkClip);

    public void PlayHitLowPitch()
    {
        if (damageClip == null) return;

        // Salva o pitch original.
        float originalPitch = sfxSource.pitch;

        // Define um pitch mais grave (menor que 1).
        sfxSource.pitch = 0.3f;

        // Toca o hitClip.
        sfxSource.PlayOneShot(damageClip);

        // Restaura o pitch original.
        sfxSource.pitch = originalPitch;
    }

    // M�todos para os sons do Boss
    public void PlayBossMode1Sound() => PlaySfx(bossMode1SFX);
    public void PlayBossMode2Sound() => PlaySfx(bossMode2SFX);
    public void PlayBossMode3Sound() => PlaySfx(bossMode3SFX);
}
