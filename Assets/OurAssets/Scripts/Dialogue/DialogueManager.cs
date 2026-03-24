using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            // Lazy instantiation
            if (!_instance)
            {
                GameObject go = new GameObject("DialogueManager");
                _instance = go.AddComponent<DialogueManager>();
            }
            return _instance;
        }
    }

    readonly QueueADT<DialogueItem> dialogueQueue = new QueueADT<DialogueItem>();

    public DialogueItem CurrentDialogueItem { get; private set; }
    public Sprite CurrentDialogueIcon { get; private set; }

    void Awake()
    {
        if (_instance && _instance != this) Destroy(gameObject);
        else _instance = this;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null) return;
        if (!dialogueQueue.IsEmpty) dialogueQueue.Clear();
        System.Array.ForEach(dialogue.dialogueItems, (item) => dialogueQueue.Enqueue(item));
        if (!dialogueQueue.IsEmpty) LoadNextItem(); // Load the first item
    }

    public void LoadNextItem()
    {
        if (dialogueQueue.IsEmpty)
        {
            if (CurrentDialogueIcon)
            {
                Resources.UnloadAsset(CurrentDialogueIcon);
                CurrentDialogueIcon = null;
            }
            CurrentDialogueItem = null;
            return;
        }
        DialogueItem nextDialogueItem = dialogueQueue.Dequeue();
        if (CurrentDialogueItem == null || CurrentDialogueItem.icon != nextDialogueItem.icon)
        {
            if (CurrentDialogueIcon) Resources.UnloadAsset(CurrentDialogueIcon);
            CurrentDialogueIcon = Resources.Load<Sprite>(nextDialogueItem.icon); // Make sure to write icons correctly
        }
        CurrentDialogueItem = nextDialogueItem;
    }
}
