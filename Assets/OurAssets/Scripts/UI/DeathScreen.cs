using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [SerializeField]
    Button restartButton;

    void Awake() => restartButton.onClick.AddListener(() => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name));
}
