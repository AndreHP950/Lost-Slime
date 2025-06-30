using System.Collections;
using UnityEngine;

public class BossPageManager : MonoBehaviour
{
    public static BossPageManager Instance { get; private set; }

    [SerializeField]
    private CanvasGroup bossPageCanvasGroup;

    [Header("Imagens Extras")]
    [Tooltip("Imagem que diz 'Continue para a próxima fase'")]
    [SerializeField] private GameObject continueImage;
    [Tooltip("Imagem que diz 'Créditos'")]
    [SerializeField] private GameObject creditsImage;

    private bool hasTriggeredExtras = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Inicialmente a página deve estar oculta.
        if (bossPageCanvasGroup != null)
        {
            bossPageCanvasGroup.alpha = 0;
            bossPageCanvasGroup.interactable = false;
            bossPageCanvasGroup.blocksRaycasts = false;
        }

        // Garante que as imagens extras estejam inativas no início.
        if (continueImage != null) continueImage.SetActive(false);
        if (creditsImage != null) creditsImage.SetActive(false);
    }

    private void Update()
    {
        // Se a imagem de créditos estiver ativa, pode fechar com ESC.
        if (creditsImage != null && creditsImage.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideBossPage();
            }
        }
    }

    public void ShowBossPage()
    {
        if (bossPageCanvasGroup != null)
        {
            bossPageCanvasGroup.alpha = 1;
            bossPageCanvasGroup.interactable = true;
            bossPageCanvasGroup.blocksRaycasts = true;
            Debug.Log("Boss Page exibida.");
        }

        // Inicia a sequência de exibição das imagens extras, somente na primeira vez.
        if (!hasTriggeredExtras)
        {
            hasTriggeredExtras = true;
            StartCoroutine(ShowExtraImagesSequence());
        }
    }

    private IEnumerator ShowExtraImagesSequence()
    {
        // Aguarda 5 segundos para mostrar a imagem de "continua".
        yield return new WaitForSeconds(5f);
        if (continueImage != null)
            continueImage.SetActive(true);

        // Aguarda mais 5 segundos para mostrar a imagem de "créditos".
        yield return new WaitForSeconds(5f);

        if (creditsImage != null)
            creditsImage.SetActive(true);
        if (continueImage != null)
            continueImage.SetActive(false);
    }

    public void HideBossPage()
    {
        if (bossPageCanvasGroup != null)
        {
            bossPageCanvasGroup.alpha = 0;
            bossPageCanvasGroup.interactable = false;
            bossPageCanvasGroup.blocksRaycasts = false;
            Debug.Log("Boss Page oculta.");
        }

        if (continueImage != null)
            continueImage.SetActive(false);
        if (creditsImage != null)
            creditsImage.SetActive(false);
    }
}
