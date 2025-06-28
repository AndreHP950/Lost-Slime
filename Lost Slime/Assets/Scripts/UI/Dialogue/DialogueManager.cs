using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Configuração de Diálogo")]
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private float typingSpeed = 0.05f;

    private Queue<DialogueLine> currentDialogue;
    private DialogueLine currentLine;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    // Referências
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerAttack;


    // Evento que ocorre ao finalizar todos os diálogos
    public System.Action onDialogueComplete;

    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string text;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        currentDialogue = new Queue<DialogueLine>();
    }

    public void StartDialogue(List<DialogueLine> lines)
    {
        // Impede o jogador de se mover durante o diálogo
        if (playerMovement != null)
            playerMovement.enabled = false;
        if (playerAttack != null)
            playerAttack.enabled = false;
        currentDialogue.Clear();
        foreach (var line in lines)
        {
            currentDialogue.Enqueue(line);
        }

        dialogueUI.ShowDialoguePanel(true);
        isDialogueActive = true;
        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (isTyping)
        {
            // Se estiver digitando, complete a linha imediatamente
            StopTypingEffect();
            dialogueUI.DisplayTextInstantly(currentLine.speaker, currentLine.text);
            return;
        }

        if (currentDialogue.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = currentDialogue.Dequeue();
        typingCoroutine = StartCoroutine(TypeTextEffect(currentLine.speaker, currentLine.text));
    }

    public void DisplayPreviousLine()
    {
        // Implementação para voltar uma linha (opcional e mais complexa)
        // Requer armazenar histórico de linhas mostradas
        Debug.Log("Voltar uma linha não implementado");
    }

    public void SkipAllDialogue()
    {
        StopTypingEffect();
        EndDialogue();
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueUI.ShowDialoguePanel(false);

        // Permite que o jogador se mova novamente
        if (playerMovement != null)
            playerMovement.enabled = true;
        if (playerAttack != null)
            playerAttack.enabled = true;
        // Notifica que o diálogo foi concluído
        onDialogueComplete?.Invoke();
    }

    private IEnumerator TypeTextEffect(string speaker, string text)
    {
        isTyping = true;
        dialogueUI.SetSpeakerName(speaker);
        dialogueUI.ClearText();

        // Lista de variações do pitch para o som de digitação
        float[] pitchVariations = new float[] { 0.9f, 0.95f, 1.0f, 1.05f, 1.1f };

        string[] words = text.Split(' ');

        for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
        {
            string word = words[wordIndex];

            // Processa cada letra da palavra
            for (int i = 0; i < word.Length; i++)
            {
                char letter = word[i];
                dialogueUI.AppendText(letter.ToString());

                // Toca o som com variação de pitch
                if (typingSound != null && AudioManager.Instance != null)
                {
                    float randomPitch = pitchVariations[Random.Range(0, pitchVariations.Length)];
                    AudioManager.Instance.PlaySfxWithPitch(typingSound, 0.5f, randomPitch);
                }

                // Variação no tempo entre letras (mais rápido no meio das palavras)
                float letterDelay = typingSpeed;
                if (i == word.Length - 1) // Última letra da palavra
                    letterDelay *= 1.5f;

                yield return new WaitForSeconds(letterDelay);
            }

            // Adiciona um espaço após cada palavra (exceto a última)
            if (wordIndex < words.Length - 1)
            {
                dialogueUI.AppendText(" ");
                // Pausa entre palavras (mais longa que entre letras)
                yield return new WaitForSeconds(typingSpeed * 2f);
            }
        }

        isTyping = false;
        typingCoroutine = null;
    }


    private void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}
