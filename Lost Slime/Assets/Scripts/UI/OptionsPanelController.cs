using UnityEngine;
using UnityEngine.UI;

public class OptionsPanelController : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject pausePanel;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            sfxSlider.value = AudioManager.Instance.GetSfxVolume();
        }

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }


    public void SetMusicVolume(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    public void SetSfxVolume(float value)
    {
        AudioManager.Instance?.SetSfxVolume(value);
    }

    public void Back()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}
