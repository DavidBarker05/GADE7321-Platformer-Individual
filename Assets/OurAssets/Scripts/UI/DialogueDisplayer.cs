using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueDisplayer : MonoBehaviour
{
    [SerializeField]
    Image dialogueIcon;
    [SerializeField]
    TextMeshProUGUI speakerName;
    [SerializeField]
    TextMeshProUGUI dialogueText;
    [SerializeField]
    Button nextButton;
    [SerializeField]
    Button skipButton;
    [SerializeField]
    Button skipAllButton;

    DialogueItem currentItem;

    bool isFinishedWithCurrentItem;
    string currentDialogueText;
    float secondsPerCharacter;
    float currentReadTime;

    void Awake()
    {
        nextButton.onClick.AddListener(
            () => {
                DialogueManager.Instance.LoadNextItem();
                RetrieveCurrentDialogue();
            }
        );
        skipButton.onClick.AddListener(() => isFinishedWithCurrentItem = true );
        skipAllButton.onClick.AddListener(() => gameObject.SetActive(false) );
    }

    void OnEnable()
    {
        RetrieveCurrentDialogue();
        // TODO: Stop character from moving and looking
    }

    void OnDisable()
    {
        // TODO: Allow character to move and look again
    }

    void Update()
    {
        if (currentItem == null) // Will this ever happen? I'm gonna leave it in case
        {
            gameObject.SetActive(false);
            return;
        }
        if (isFinishedWithCurrentItem)
        {
            if (dialogueText.text != currentItem.text) dialogueText.text = currentItem.text;
            if (!nextButton.gameObject.activeSelf) nextButton.gameObject.SetActive(true);
            return;
        }
        if (currentItem.charactersPerSecond < 0) dialogueText.text = currentItem.text;
        else
        {
            currentReadTime += Time.deltaTime;
            if (currentReadTime >= secondsPerCharacter)
            {
                int numNewCharacters = (int)(currentReadTime / secondsPerCharacter);
                dialogueText.text = currentItem.text[..Mathf.Clamp(dialogueText.text.Length + numNewCharacters, 0, currentItem.text.Length)];
                currentReadTime = 0f;
            }
        }
        dialogueText.text = currentDialogueText;
        isFinishedWithCurrentItem = dialogueText.text.Length == currentItem.text.Length;
    }

    void RetrieveCurrentDialogue()
    {
        currentItem = DialogueManager.Instance.CurrentDialogueItem;
        if (currentItem == null)
        {
            gameObject.SetActive(false);
            return;
        }
        nextButton.gameObject.SetActive(false);
        dialogueIcon.sprite = DialogueManager.Instance.CurrentDialogueIcon;
        speakerName.text = currentItem.speakerName;
        dialogueText.text = "";
        dialogueText.fontSize = currentItem.fontSize;
        isFinishedWithCurrentItem = false;
        currentDialogueText = "";
        secondsPerCharacter = 1f / currentItem.charactersPerSecond;
        currentReadTime = 0f;
    }
}
