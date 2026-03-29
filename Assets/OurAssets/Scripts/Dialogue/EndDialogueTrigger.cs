using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDialogueTrigger : DialogueTrigger
{
    [SerializeField, Min(0)]
    int notEnoughItems = 0;
    [SerializeField, Min(0)]
    int endDialogue = 1;
    [SerializeField]
    int nextSceneIndex = -1;

    protected override void OnTriggerEnter(Collider other)
    {
        Debug.Assert(CollectableManager.Instance, "CollectableManger must exist in the scene");
        if (!other.CompareTag("Player") || !CollectableManager.Instance) return;
        if (CollectableManager.Instance.HasEnoughItems) GetComponent<DialogueHolder>().StartDialogue(endDialogue, EndOfLevelFunction);
        else GetComponent<DialogueHolder>().StartDialogue(notEnoughItems, MovePlayerOffFinalPlatform);
    }

    protected void EndOfLevelFunction()
    {
        if (nextSceneIndex > -1) SceneManager.LoadSceneAsync(nextSceneIndex); // Maybe we even do loading screen idk this is fine for now
        else
        {
            // For now just close game once we have a main menu then we can go back or restart
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    protected void MovePlayerOffFinalPlatform()
    {
        // TODO: move player off end of final platform. Maybe I keep track of last ground position?
        // Or maybe respawn at checkpoint?
    }
}
