using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private AudioClip playButtonClip;

    public void Play()
    {
        StartCoroutine(PlayAndLoad());
    }

    private IEnumerator PlayAndLoad()
    {
        // Verifica se o AudioManager existe e se o clipe está atribuído
        if (AudioManager.Instance != null && playButtonClip != null)
        {
            AudioManager.Instance.PlaySfx(playButtonClip);
            yield return new WaitForSeconds(playButtonClip.length);
        }
        else
        {
            // Se não houver AudioManager ou clipe, apenas espera um pequeno tempo para evitar clique duplo
            yield return new WaitForSeconds(0.1f);
        }
        SceneManager.LoadScene("Level1");
    }

    public void Options()
    {
        // SceneManager.LoadScene("Options");
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
