using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MenuPanel
{
    [SerializeField] private Button _btnBack;

    public event Action OnBackClicked;

    private void Awake()
    {
        _btnBack.onClick.AddListener(() => OnBackClicked?.Invoke());
    }
}