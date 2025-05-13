using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;        
using UnityEngine.UI;
using Unity.Cinemachine;

public class PlayerDeathController : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private Health health;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAttack attack;

    [Header("Cinemachine")]
    [Tooltip("Your scene's Cinemachine Camera component")]
    [SerializeField] private CinemachineCamera vCam;  
    [Tooltip("How long the zoom/shake lasts")]
    [SerializeField] private float deathEffectDuration = 0.8f;
    [Tooltip("Field-of-view when zoomed in")]
    [SerializeField] private float zoomFOV = 40f;
    [Tooltip("Shake amplitude")]
    [SerializeField] private float shakeAmplitude = 3f;

    [Header("UI")]
    [Tooltip("Root panel with black overlay + YOU DIED text")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Canvas mainCanvas; // Referência ao Canvas principal

    private float originalFOV;
    private CinemachineBasicMultiChannelPerlin perlin;

    void Awake()
    {
        // auto-grab if nothing was assigned
        health ??= GetComponent<Health>();
        movement ??= GetComponent<PlayerMovement>();
        attack ??= GetComponent<PlayerAttack>();

        // hide death UI
        if (deathPanel != null) deathPanel.SetActive(false);

        // listen for death
        health.onDied.AddListener(OnPlayerDied);

        // cinemachine setup
        if (vCam != null)
        {
            originalFOV = vCam.Lens.FieldOfView;
            perlin = vCam.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
            if (perlin != null)
                perlin.AmplitudeGain = 0f;      // <–– aqui desligamos todo shake no início
        
    }
    }

    private void OnPlayerDied()
    {
        // Desativa controles
        movement.enabled = false;
        attack.enabled = false;

        // Desativa todos os filhos do Canvas, exceto o deathPanel
        if (mainCanvas != null)
        {
            foreach (Transform child in mainCanvas.transform)
            {
                if (deathPanel != null && child.gameObject == deathPanel)
                    continue;
                child.gameObject.SetActive(false);
            }
        }

        // Ativa o painel de morte
        if (deathPanel != null)
            deathPanel.SetActive(true);

        // Efeito de câmera
        if (vCam != null)
            StartCoroutine(DeathCameraEffect());
    }

    private IEnumerator DeathCameraEffect()
    {
        // zoom in
        vCam.Lens.FieldOfView = zoomFOV;
        // start shake
        if (perlin != null)
            perlin.AmplitudeGain = shakeAmplitude;

        yield return new WaitForSeconds(deathEffectDuration);

        // restore FOV & stop shake
        vCam.Lens.FieldOfView = originalFOV;
        if (perlin != null)
            perlin.AmplitudeGain = 0f;
    }

    void Update()
    {
        // once dead-panel is up, Space reloads current scene
        if (deathPanel != null && deathPanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
