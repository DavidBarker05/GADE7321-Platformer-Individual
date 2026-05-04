using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputScript))]
public class PlayerPause : MonoBehaviour
{
	[SerializeField]
	GameObject m_PauseScreen;
	
	public bool Paused { get; private set; } = false;

    PlayerInputScript m_PlayerInputScript;

	void Awake()
	{
		m_PlayerInputScript = GetComponent<PlayerInputScript>();
		InputSystem.actions.FindAction("TogglePause").started += TogglePause;
	}

	void OnDestroy() => InputSystem.actions.FindAction("TogglePause").started -= TogglePause;
	
	void TogglePause(InputAction.CallbackContext ctx)
	{
		if (!m_PauseScreen)
		{
			Debug.LogWarning("Pause screen is missing");
			return;
		}
		if (Paused && Time.timeScale == 0f) UnpauseGame(); // Unpause if this class paused the game
		else if (!Paused && Time.timeScale == 1f) PauseGame(); // Pause if the game is unpaused
	}

	void PauseGame()
	{
		Paused = true;
		m_PauseScreen.SetActive(true);
		m_PlayerInputScript.DisableCharacterInput(); // Disable input and pause game for menu
	}

	public void UnpauseGame() // public so that resume button can access it
	{
		Paused = false;
		m_PauseScreen.SetActive(false);
		m_PlayerInputScript.EnableCharacterInput(); // Re-enable input and resume game
	}
}
