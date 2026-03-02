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
        // Will this work? Gotta test. Been a while since worked with C# lambdas.
        // idk if I can use variables that belong to the class or if I need to capture
        // them like in c++
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
