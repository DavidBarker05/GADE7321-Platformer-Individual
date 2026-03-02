using System.IO;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField]
    string dialogueFolder = "Dialogue";
    [SerializeField]
    string jsonFolder = "DialogueJSONs";
    [SerializeField]
    string iconFolder = "DialogueIcons";

    public DialogueItem CurrentDialogueItem { get; private set; }
    public Sprite CurrentDialogueIcon { get; private set; }

    QueueADT<DialogueItem> dialogueQueue = new QueueADT<DialogueItem>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    T LoadResource<T>(string folder, string fileName) where T : Object
    {
        string resource = Path.Combine(dialogueFolder, folder, fileName);
        string path = Path.Combine(Application.dataPath, resource);
        if (File.Exists(path)) return Resources.Load<T>(resource);
        Debug.LogError($"ERROR: \"{fileName}\" doesn't exist");
        return null;
    }

    public Dialogue LoadDialogue(string fileName)
    {
        if (!fileName.EndsWith(".json")) fileName += ".json";
        TextAsset json = LoadResource<TextAsset>(jsonFolder, fileName);
        Dialogue dialogue = null;
        if (json != null) dialogue = JsonUtility.FromJson<Dialogue>(json.text);
        return dialogue;
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
        CurrentDialogueIcon = LoadResource<Sprite>(iconFolder, CurrentDialogueItem.icon);
    }
}
