using UnityEngine;

public class DialogueHolder : MonoBehaviour
{
    [SerializeField]
    string dialogueFileName;
    [SerializeField]
    bool canPlayMultipleTimes = false;

    Dialogue dialogue = null;
    bool hasBeenPlayed = false;

    void Start()
    {
        if (dialogue == null) TryLoadDialogue();
    }

    public void StartDialogue()
    {
        if (hasBeenPlayed && !canPlayMultipleTimes) return;
        hasBeenPlayed = true;
        if (dialogue == null) TryLoadDialogue(); // Added in case StartDialogue gets called before Start
        DialogueManager.Instance?.StartDialogue(dialogue);
    }

    void TryLoadDialogue() => dialogue = DialogueLoader.Instance?.LoadDialogue(dialogueFileName);
}
