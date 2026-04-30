using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueDisplayer : MonoBehaviour
{
    [SerializeField]
    Image dialogueIcon;
    [SerializeField]
	TMP_Text speakerName;  // David - Use TMP_Text instead of TextMeshProUGUI so that isn't only forced for UI
	[SerializeField]
    TMP_Text dialogueText; // David - Use TMP_Text instead of TextMeshProUGUI so that isn't only forced for UI
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

    float secondsPerCharacter;
    float currentReadTime;

    System.Action endCallbackFunction;

	void Awake()
	{
		playerInput = FindFirstObjectByType<PlayerInputScript>();
		nextButton.onClick.AddListener(NextDialogueItem);
		skipButton.onClick.AddListener(SkipCurrentItem);
		skipAllButton.onClick.AddListener(StopDisplayingDialogue);
		speakerAnchorMin = speakerName.rectTransform.anchorMin;
		speakerAnchorMax = speakerName.rectTransform.anchorMax;
		speakerPivot = speakerName.rectTransform.pivot;
		speakerAnchoredPosition = speakerName.rectTransform.anchoredPosition;
		speakerSize = speakerName.rectTransform.sizeDelta;
		iconSize = dialogueIcon.rectTransform.sizeDelta;
		gameObject.SetActive(false);
	}

	void OnDestroy()
	{
		nextButton.onClick.RemoveListener(NextDialogueItem);
		skipButton.onClick.RemoveListener(SkipCurrentItem);
		skipAllButton.onClick.RemoveListener(StopDisplayingDialogue);
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
			if (dialogueText.maxVisibleCharacters != dialogueText.text.Length) dialogueText.maxVisibleCharacters = dialogueText.text.Length;
			if (!nextButton.gameObject.activeSelf) nextButton.gameObject.SetActive(true);
			return;
		}
		if (currentItem.charactersPerSecond < 0) dialogueText.maxVisibleCharacters = dialogueText.text.Length;
		else
		{
			currentReadTime += Time.deltaTime;
			if (currentReadTime >= secondsPerCharacter)
			{
			    int numNewCharacters = (int)(currentReadTime / secondsPerCharacter);
				dialogueText.maxVisibleCharacters = Mathf.Clamp(dialogueText.maxVisibleCharacters + numNewCharacters, 0, dialogueText.text.Length);
			    currentReadTime = 0f;
			}
		}
		isFinishedWithCurrentItem = dialogueText.maxVisibleCharacters == dialogueText.text.Length;
	}

	void NextDialogueItem()
	{
		DialogueManager.Instance.LoadNextItem();
		RetrieveCurrentDialogue();
	}

	void SkipCurrentItem() => isFinishedWithCurrentItem = true;

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
	    dialogueText.text = currentItem.text;
	    dialogueText.fontSize = currentItem.fontSize;
	    isFinishedWithCurrentItem = false;
		dialogueText.maxVisibleCharacters = 0;
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
