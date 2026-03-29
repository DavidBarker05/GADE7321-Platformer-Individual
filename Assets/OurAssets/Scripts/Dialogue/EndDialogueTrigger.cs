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

    protected bool EndConditionSingletonsAreValid => CollectableManager.Instance;
    protected bool EndConditionsAreMet => CollectableManager.Instance.HasEnoughItems;

    PlayerMovement player;

    protected override void OnTriggerEnter(Collider other)
    {
        Debug.Assert(CollectableManager.Instance, "CollectableManger must exist in the scene");
        if (!other.CompareTag("Player") || !EndConditionSingletonsAreValid) return;
        if (EndConditionsAreMet) GetComponent<DialogueHolder>().StartDialogue(endDialogue, EndOfLevelFunction);
        else
        {
            player = other.gameObject.GetComponent<PlayerMovement>();
            GetComponent<DialogueHolder>().StartDialogue(notEnoughItems, ResetPlayerToLastStableGround);
        }
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

    protected void ResetPlayerToLastStableGround() => player?.ResetToLastStableGround();
}
