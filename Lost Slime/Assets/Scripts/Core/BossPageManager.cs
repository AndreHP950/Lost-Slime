using UnityEngine;

public class BossPageManager : MonoBehaviour
{
    public static BossPageManager Instance { get; private set; }

    [SerializeField]
    private CanvasGroup bossPageCanvasGroup;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
    }
}
