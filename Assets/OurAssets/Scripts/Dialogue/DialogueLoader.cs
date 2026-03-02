using System.IO;
using UnityEngine;

public class DialogueLoader : MonoBehaviour
{
    public static DialogueLoader Instance { get; private set; }

    [SerializeField]
    string dialogueFolder = "Dialogue";
    [SerializeField]
    string jsonFolder = "DialogueJSONs";
    [SerializeField]
    string iconFolder = "DialogueIcons";

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

    public Sprite LoadIcon(DialogueItem dialogueItem) => LoadResource<Sprite>(iconFolder, dialogueItem.icon);
}
