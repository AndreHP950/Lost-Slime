using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private float delayBeforeDialogue = 1.0f;
    private bool hasTriggeredDialogue = false;
    private bool hasPlayerMoved = false;
    private PlayerMovement playerMovement;
    private float playerMoveTimer = 0f;

    [Header("Diálogo")]
    [TextArea(3, 10)]
    [SerializeField] private List<string> speakers = new List<string>();
    [TextArea(3, 10)]
    [SerializeField] private List<string> dialogueLines = new List<string>();

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!hasTriggeredDialogue)
        {
            // Verifica se o jogador se moveu
            if (playerMovement.inputVector.magnitude > 0.1f)
            {
                hasPlayerMoved = true;
                playerMoveTimer += Time.deltaTime;

                // Após o delay, inicia o diálogo
                if (playerMoveTimer >= delayBeforeDialogue && !DialogueManager.Instance.IsDialogueActive())
                {
                    TriggerDialogue();
                }
            }
        }
    }

    private void TriggerDialogue()
    {
        if (hasTriggeredDialogue)
            return;

        hasTriggeredDialogue = true;

        // Prepara o diálogo com base nos textos configurados
        List<DialogueManager.DialogueLine> lines = new List<DialogueManager.DialogueLine>();

        for (int i = 0; i < dialogueLines.Count; i++)
        {
            string speaker = "";
            // Se temos um speaker correspondente, use-o
            if (i < speakers.Count && !string.IsNullOrEmpty(speakers[i]))
                speaker = speakers[i];

            var line = new DialogueManager.DialogueLine
            {
                speaker = speaker,
                text = dialogueLines[i]
            };
            lines.Add(line);
        }

        // Inicia o diálogo
        DialogueManager.Instance.StartDialogue(lines);
    }

}
