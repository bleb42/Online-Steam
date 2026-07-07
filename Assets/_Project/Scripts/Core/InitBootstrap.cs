using UnityEngine;
using UnityEngine.SceneManagement;

public class InitBootstrap : MonoBehaviour
{
    private void Start()
    {
        var operation = SceneManager.LoadSceneAsync("Menu");
        operation.completed += OnMenuLoaded;
    }

    private void OnMenuLoaded(AsyncOperation operation)
    {
        ScreenFader.Instance.FadeOut();
    }
}