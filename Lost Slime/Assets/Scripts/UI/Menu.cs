using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private AudioClip playButtonClip;
    [SerializeField] private GameObject optionsPanel; // arraste o OptionsPanel aqui
    [SerializeField] private GameObject mainMenuPanel; // painel principal do menu (os botões Play/Options/Exit)
    void Awake()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }


    public void Play()
    {
        StartCoroutine(PlayAndLoad());
    }

    private IEnumerator PlayAndLoad()
    {
        if (AudioManager.Instance != null && playButtonClip != null)
        {
            AudioManager.Instance.PlaySfx(playButtonClip);
            yield return new WaitForSeconds(playButtonClip.length);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }
        SceneManager.LoadScene("Level1");
    }

    public void Options()
    {
        if (optionsPanel != null )
        {
            optionsPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
        }
    }

    public void BackFromOptions()
    {
        if (optionsPanel != null && mainMenuPanel != null)
        {
            optionsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
