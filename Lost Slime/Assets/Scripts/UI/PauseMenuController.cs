using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button prototypeButton;
    [SerializeField] private Button level1Button;

    [Header("Pause Overlay")]
    [SerializeField] private Image pauseVignette;    // Overlay para escurecer a tela
    [SerializeField] private float pauseVignetteAlpha = 0.3f; // Valor de alpha para o overlay

    [Header("Scene Elements")]
    [SerializeField] private GameObject canvasObject;  // Referência ao Canvas
    [SerializeField] private GameObject groundObject;  // Referência ao chão com bake do NavMesh

    private bool isPaused = false;

    void Start()
    {
        // Desativa o painel de pause e o overlay ao iniciar
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (pauseVignette != null)
            pauseVignette.color = new Color(0, 0, 0, 0);

        // Ativa o Canvas e o chão automaticamente ao iniciar o jogo

            canvasObject.SetActive(true);

            groundObject.SetActive(true);

        // Configura o slider de volume
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Configura os botões de troca de cena com SFX ao clicar
        if (menuButton != null)
            menuButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayButton();
                TryLoadScene("Menu");
            });
        if (prototypeButton != null)
            prototypeButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayButton();
                TryLoadScene("PrototypeLevel");
            });
        if (level1Button != null)
            level1Button.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayButton();
                TryLoadScene("Level1");
            });
    }

    void Update()
    {
        // Na cena "Menu" não ativamos o pause, então ESC não funciona
        if (SceneManager.GetActiveScene().name == "Menu")
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                Pause();
            else
                Resume();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null)
            pausePanel.SetActive(true);
        if (pauseVignette != null)
            pauseVignette.color = new Color(0, 0, 0, pauseVignetteAlpha);
        // Opcional: Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (pauseVignette != null)
            pauseVignette.color = new Color(0, 0, 0, 0);
        // Opcional: Cursor.visible = false;
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    // Método para iniciar o jogo
    public void StartGame()
    {
        // Ativa Canvas e chão quando o jogo iniciar
        if (canvasObject != null)
            canvasObject.SetActive(true);
        if (groundObject != null)
            groundObject.SetActive(true);

        // Fecha o menu e retoma o jogo (se necessário)
        Resume();
    }

    // Tenta carregar a cena desejada. Se já estiver na cena, apenas fecha o painel.
    public void TryLoadScene(string sceneName)
    {
        Time.timeScale = 1f; // Restaura a escala de tempo antes de trocar de cena
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            Resume();
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
