using System;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [SerializeField] DialogueUI dialogueUI;

    Player currentPlayer;
    string currentSpeaker;
    string[] currentLines;
    int lineIndex;
    Action onDialogueEnded;

    public bool IsDialogueActive { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
    }

    public void StartDialogue(Player player, string speakerName, string[] lines)
    {
        StartDialogue(player, speakerName, lines, null, null);
    }

    public void StartDialogue(Player player, string speakerName, string[] lines, Transform dialogueAnchor, Action onEnded)
    {
        if (dialogueUI == null)
        {
            Debug.LogWarning("DialogueUI가 연결되지 않았습니다.");
            return;
        }

        if (lines == null || lines.Length == 0)
            return;

        currentPlayer = player;
        currentSpeaker = speakerName;
        currentLines = lines;
        lineIndex = 0;
        IsDialogueActive = true;
        onDialogueEnded = onEnded;

        currentPlayer?.SetInputLocked(true);
        dialogueUI.Open(dialogueAnchor);
        dialogueUI.ShowLine(currentSpeaker, currentLines[lineIndex]);
    }

    public void Advance()
    {
        if (!IsDialogueActive || dialogueUI == null)
            return;

        if (dialogueUI.IsTyping)
        {
            dialogueUI.CompleteLine();
            return;
        }

        lineIndex++;
        if (lineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueUI.ShowLine(currentSpeaker, currentLines[lineIndex]);
    }

    void EndDialogue()
    {
        IsDialogueActive = false;
        dialogueUI.Close();
        currentPlayer?.SetInputLocked(false);
        onDialogueEnded?.Invoke();

        currentPlayer = null;
        currentSpeaker = null;
        currentLines = null;
        lineIndex = 0;
        onDialogueEnded = null;
    }
}
