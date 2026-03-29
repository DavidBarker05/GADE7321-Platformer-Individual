using UnityEngine;

public class DialogueHolder : MonoBehaviour
{
    [SerializeField]
    TextAsset[] dialogueFiles;
    [SerializeField]
    bool canPlayMultipleTimes = false;
    [SerializeField]
    DialogueDisplayer displayer;

    bool hasBeenPlayed = false;

    public void StartDialogue(int dialogueNumber = 0, System.Action callbackFunction = null)
    {
        if (hasBeenPlayed && !canPlayMultipleTimes || dialogueNumber > dialogueFiles.Length) return;
        hasBeenPlayed = true;
        Dialogue dialogue = JsonUtility.FromJson<Dialogue>(dialogueFiles[dialogueNumber].text);
        DialogueManager.Instance.StartDialogue(dialogue);
        displayer.StartDisplayingDialogue(callbackFunction);
    }

    public void StartDialogue(System.Action callbackFunction) => StartDialogue(0, callbackFunction);
}
