using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel; // NOVO
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button prototypeButton;
    [SerializeField] private Button level1Button;
    [SerializeField] private Button optionsButton; // NOVO

    [Header("Pause Overlay")]
    [SerializeField] private Image pauseVignette;
    [SerializeField] private float pauseVignetteAlpha = 0.3f;

    [Header("Scene Elements")]
    [SerializeField] private GameObject canvasObject;
    [SerializeField] private GameObject groundObject;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        if (pauseVignette != null)
            pauseVignette.color = new Color(0, 0, 0, 0);

        if (canvasObject != null)
            canvasObject.SetActive(true);
        if (groundObject != null)
            groundObject.SetActive(true);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

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

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                // Se está no painel de opções, volta para o pause
                optionsPanel.SetActive(false);
                pausePanel.SetActive(true);
            }
            else
            {
                if (!isPaused)
                    Pause();
                else
                    Resume();
            }
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
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        if (pauseVignette != null)
            pauseVignette.color = new Color(0, 0, 0, 0);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void StartGame()
    {
        if (canvasObject != null)
            canvasObject.SetActive(true);
        if (groundObject != null)
            groundObject.SetActive(true);
        Resume();
    }

    public void TryLoadScene(string sceneName)
    {
        Time.timeScale = 1f;
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
