using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NpcDialogue : MonoBehaviour, IInteractable
{
    [SerializeField] string speakerName = "NPC";
    [SerializeField] string promptText = "Talk";
    [SerializeField] GameObject promptObject;
    [SerializeField] Transform dialogueAnchor;
    [TextArea(2, 4)]
    [SerializeField] string[] lines;

    bool playerInRange;
    bool dialogueActive;

    public string PromptText => promptText;

    void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;

        if (promptObject != null)
        {
            TMP_Text promptLabel = promptObject.GetComponentInChildren<TMP_Text>(true);
            if (promptLabel != null)
                promptLabel.text = promptText;
        }

        SetPrompt(false);
    }

    public void Interact(Player player)
    {
        if (DialogueController.Instance == null)
        {
            Debug.LogWarning("DialogueController가 씬에 없습니다.");
            return;
        }

        dialogueActive = true;
        SetPrompt(false);
        DialogueController.Instance.StartDialogue(player, speakerName, lines, dialogueAnchor != null ? dialogueAnchor : transform, OnDialogueEnded);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInteractor interactor = FindPlayerInteractor(other);
        if (interactor == null)
            return;

        playerInRange = true;
        interactor.Register(this);
        SetPrompt(!dialogueActive);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerInteractor interactor = FindPlayerInteractor(other);
        if (interactor == null)
            return;

        playerInRange = false;
        interactor.Unregister(this);
        SetPrompt(false);
    }

    PlayerInteractor FindPlayerInteractor(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
            return player.GetComponentInChildren<PlayerInteractor>(true);

        return other.GetComponentInParent<PlayerInteractor>();
    }

    void OnDialogueEnded()
    {
        dialogueActive = false;
        SetPrompt(playerInRange);
    }

    void SetPrompt(bool visible)
    {
        if (promptObject != null)
            promptObject.SetActive(visible);
    }
}
