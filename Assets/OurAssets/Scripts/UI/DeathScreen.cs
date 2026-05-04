using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [SerializeField]
    LoadingScreen loadingScreen;
    [SerializeField]
    Button restartButton;

    void Awake()
    {
        restartButton.onClick.AddListener(() => {
            loadingScreen.SceneIndexToLoad = LevelStartManager.Instance.StartingLevelIndex; // Restart at the scene that was chosen in level select because don't want to have to keep opening menu
            loadingScreen.gameObject.SetActive(true);
        });
    }

    void OnEnable()
    {
        PlayerInputScript pis = FindFirstObjectByType<PlayerInputScript>();
        pis.DisableCharacterInput();
        pis.GetComponentInChildren<Renderer>().enabled = false; // Hide character model on death
    }
}
