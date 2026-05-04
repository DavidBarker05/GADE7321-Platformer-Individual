using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TimeDisplay : MonoBehaviour
{
	[SerializeField, TextArea]
	string m_DisplayText;

	TMP_Text m_Text;

	void Awake() => m_Text = GetComponent<TMP_Text>();

	void Update() => m_Text.text = string.Format(m_DisplayText, LevelTimer.Instance?.CurrentTime ?? 0);
}
