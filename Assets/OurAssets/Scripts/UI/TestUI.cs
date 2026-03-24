using UnityEngine;
using TMPro;

// This is a test script for UI. This is just where I can write
// code to test if different UI features are working before I
// know what to do for final implementation
public class TestUI : MonoBehaviour
{
    TextMeshProUGUI tmpro;

    void Awake()
    {
        tmpro = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (DialogueManager.Instance.CurrentDialogueItem == null) return;
        tmpro.text = DialogueManager.Instance.CurrentDialogueItem.text;
        tmpro.fontSize = DialogueManager.Instance.CurrentDialogueItem.fontSize;
    }
}
