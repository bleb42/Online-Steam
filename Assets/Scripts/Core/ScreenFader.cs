using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private Image _image;
    [SerializeField] private float _duration = 0.5f;

    private void Awake()
    {
        if (Instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void FadeIn(Action onComplete = null) => StartCoroutine(Fade(0f, 1f, onComplete));
    public void FadeOut(Action onComplete = null) => StartCoroutine(Fade(1f, 0f, onComplete));

    private IEnumerator Fade(float from, float to, Action onComplete)
    {
        float time = 0f;
        Color color = _image.color;
        color.a = from;
        _image.color = color;

        while (time < _duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(from, to, time / _duration);
            _image.color = color;
            yield return null;
        }

        color.a = to;
        _image.color = color;
        onComplete?.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") 
            return;

        FadeOut();
    }
}