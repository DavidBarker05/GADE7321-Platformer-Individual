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

    Vector2 speakerAnchorMin;
    Vector2 speakerAnchorMax;
    Vector2 speakerPivot;
    Vector2 speakerAnchoredPosition;
    Vector2 speakerSize;

    Vector2 iconSize;

    PlayerInputScript playerInput;

    DialogueItem currentItem;

    bool isFinishedWithCurrentItem;

    string currentDialogueText;
    int currentReadIndex;

    float secondsPerCharacter;
    float currentReadTime;

    System.Action endCallbackFunction;

    void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInputScript>();
        nextButton.onClick.AddListener(
            () => {
                DialogueManager.Instance.LoadNextItem();
                RetrieveCurrentDialogue();
            }
        );
        skipButton.onClick.AddListener(() => isFinishedWithCurrentItem = true);
        skipAllButton.onClick.AddListener(() => StopDisplayingDialogue());
        Debug.Log(speakerName.rectTransform.anchorMin);
        speakerAnchorMin = speakerName.rectTransform.anchorMin;
        speakerAnchorMax = speakerName.rectTransform.anchorMax;
        speakerPivot = speakerName.rectTransform.pivot;
        speakerAnchoredPosition = speakerName.rectTransform.anchoredPosition;
        speakerSize = speakerName.rectTransform.sizeDelta;
        iconSize = dialogueIcon.rectTransform.sizeDelta;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentItem == null) // Will this ever happen? I'm gonna leave it in case
        {
            StopDisplayingDialogue();
            return;
        }
        if (isFinishedWithCurrentItem)
        {
            if (dialogueText.text != currentItem.text) dialogueText.text = currentItem.text;
            if (!nextButton.gameObject.activeSelf) nextButton.gameObject.SetActive(true);
            return;
        }
        if (currentItem.charactersPerSecond < 0) currentDialogueText = currentItem.text;
        else
        {
            currentReadTime += Time.deltaTime;
            if (currentReadTime >= secondsPerCharacter)
            {
                int numNewCharacters = (int)(currentReadTime / secondsPerCharacter);
                ReadDialogue(numNewCharacters);
                //Debug.Log(currentItem.text[..1]);
                //currentDialogueText = currentItem.text[..Mathf.Clamp(currentDialogueText.Length + numNewCharacters, 0, currentItem.text.Length)];
                currentReadTime = 0f;
            }
        }
        dialogueText.text = currentDialogueText;
        isFinishedWithCurrentItem = dialogueText.text.Length == currentItem.text.Length;
    }

    string CurrentTextToDisplay(int numCharacters) => currentItem.text[..Mathf.Clamp(currentReadIndex + numCharacters, 0, currentItem.text.Length)];

    void ReadDialogue(int numCharacters)
    {
        for (int i = 0; i < numCharacters; ++i)
        {
            if (currentReadIndex + 2 < currentItem.text.Length && currentItem.text.Substring(currentReadIndex, 3) == "<i>")
            {
                currentDialogueText = CurrentTextToDisplay(3);
                currentReadIndex += 3;
                i += 2;
            }
            else if (currentReadIndex + 3 < currentItem.text.Length && currentItem.text.Substring(currentReadIndex, 4) == "</i>")
            {
                currentDialogueText = CurrentTextToDisplay(4);
                currentReadIndex += 4;
                i += 3;
            }
            else
            {
                currentDialogueText = CurrentTextToDisplay(1);
                ++currentReadIndex;
            }
        }
    }

    void DisplaySpeakerAndIcon()
    {
        speakerName.rectTransform.anchorMin = speakerAnchorMin;
        speakerName.rectTransform.anchorMax = speakerAnchorMax;
        speakerName.rectTransform.pivot = speakerPivot;
        speakerName.rectTransform.anchoredPosition = speakerAnchoredPosition;
        speakerName.rectTransform.sizeDelta = speakerSize;
        dialogueIcon.gameObject.SetActive(true);
    }

    void DisplaySpeaker()
    {
        speakerName.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        speakerName.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        speakerName.rectTransform.pivot = new Vector2(0f, 0.5f);
        speakerName.rectTransform.anchoredPosition = new Vector2(speakerName.rectTransform.anchoredPosition.x, 0f);
        speakerName.rectTransform.sizeDelta = iconSize;
        dialogueIcon.gameObject.SetActive(false);
    }

    void RetrieveCurrentDialogue()
    {
        currentItem = DialogueManager.Instance.CurrentDialogueItem;
        if (currentItem == null)
        {
            StopDisplayingDialogue();
            return;
        }
        nextButton.gameObject.SetActive(false);
        dialogueIcon.sprite = DialogueManager.Instance.CurrentDialogueIcon;
        speakerName.text = currentItem.name;
        if (dialogueIcon.sprite) DisplaySpeakerAndIcon();
        else DisplaySpeaker();
        dialogueText.text = "";
        dialogueText.fontSize = currentItem.fontSize;
        isFinishedWithCurrentItem = false;
        currentDialogueText = "";
        currentReadIndex = 0;
        secondsPerCharacter = 1f / currentItem.charactersPerSecond;
        currentReadTime = 0f;
    }

    public void StartDisplayingDialogue(System.Action callbackFunction = null)
    {
        playerInput ??= FindFirstObjectByType<PlayerInputScript>();
        playerInput?.DisableCharacterInput();
        gameObject.SetActive(true);
        RetrieveCurrentDialogue();
        endCallbackFunction = callbackFunction;
    }

    void StopDisplayingDialogue()
    {
        playerInput?.EnableCharacterInput();
        endCallbackFunction?.Invoke();
        endCallbackFunction = null; // Clear the callback function (is this even needed since we set it to null on start? Idk I'll leave it to be safe)
        gameObject.SetActive(false);
    }
}
