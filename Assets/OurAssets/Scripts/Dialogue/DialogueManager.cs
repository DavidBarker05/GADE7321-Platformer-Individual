using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    readonly QueueADT<DialogueItem> dialogueQueue = new QueueADT<DialogueItem>();

    public DialogueItem CurrentDialogueItem { get; private set; }
    public Sprite CurrentDialogueIcon { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null) return;
        System.Array.ForEach(dialogue.dialogueItems, (item) => dialogueQueue.Enqueue(item));
        if (!dialogueQueue.IsEmpty) LoadNextItem(); // Load the first item
    }

    public void LoadNextItem()
    {
        if (CurrentDialogueIcon != null) Resources.UnloadAsset(CurrentDialogueIcon);
        CurrentDialogueItem = dialogueQueue.Dequeue();
        CurrentDialogueIcon = DialogueLoader.Instance?.LoadIcon(CurrentDialogueItem);
    }
}
