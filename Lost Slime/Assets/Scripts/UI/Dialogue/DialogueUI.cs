using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Button References")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button skipAllButton;

    private void Start()
    {
        // Configura os listeners dos botões
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);

        if (previousButton != null)
            previousButton.onClick.AddListener(OnPreviousButtonClicked);

        if (skipAllButton != null)
            skipAllButton.onClick.AddListener(OnSkipAllButtonClicked);

        // Oculta o painel de diálogo no início
        ShowDialoguePanel(false);
    }
    private void Update()
    {
        if (DialogueManager.Instance.IsDialogueActive())
        {
            // Avançar com Espaço/Enter
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnNextButtonClicked();
            }
            // Voltar com Backspace/Seta esquerda
            else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnPreviousButtonClicked();
            }
            // Pular tudo com Escape
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnSkipAllButtonClicked();
            }
        }
    }

    public void ShowDialoguePanel(bool show)
    {
        dialoguePanel.SetActive(show);
    }

    public void SetSpeakerName(string speaker)
    {
        speakerText.text = speaker;
    }

    public void ClearText()
    {
        dialogueText.text = "";
    }

    public void AppendText(string text)
    {
        dialogueText.text += text;
    }

    public void DisplayTextInstantly(string speaker, string text)
    {
        speakerText.text = speaker;
        dialogueText.text = text;
    }

    private void OnNextButtonClicked()
    {
        DialogueManager.Instance.DisplayNextLine();
    }

    private void OnPreviousButtonClicked()
    {
        DialogueManager.Instance.DisplayPreviousLine();
    }

    private void OnSkipAllButtonClicked()
    {
        DialogueManager.Instance.SkipAllDialogue();
    }
}
