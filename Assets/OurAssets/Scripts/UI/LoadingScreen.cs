using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [field: SerializeField, Min(0)]
    public int SceneIndexToLoad { get; set; } = 0; // public set for later so that can choose what scene to load via script
    [SerializeField]
    Slider loadingBar;

    private void OnEnable() => StartCoroutine(LoadAsync());

    IEnumerator LoadAsync()
    {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(SceneIndexToLoad);
        while (!loadScene.isDone)
        {
            float loadProgress = Mathf.Clamp01(loadScene.progress);
            loadingBar.value = loadProgress;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
