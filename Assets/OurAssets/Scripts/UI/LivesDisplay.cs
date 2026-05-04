using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LivesDisplay : MonoBehaviour
{
    [SerializeField, TextArea]
    string m_DisplayText;

    TMP_Text m_Text;

	void Awake() => m_Text = GetComponent<TMP_Text>();

    void Update() => m_Text.text = string.Format(m_DisplayText, CheckpointManager.Instance.Lives); // Is calling this every frame a good idea? I'm sure it'll be fine...
}
