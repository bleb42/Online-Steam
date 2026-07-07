using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MenuPanel
{
    [SerializeField] private Button _btnMultiplayer;
    [SerializeField] private Button _btnSettings;

    public event Action OnMultiplayerClicked;
    public event Action OnSettingsClicked;

    private void Awake()
    {
        _btnMultiplayer.onClick.AddListener(() => OnMultiplayerClicked?.Invoke());
        _btnSettings.onClick.AddListener(() => OnSettingsClicked?.Invoke());
    }
}