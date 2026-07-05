using System;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerPanel : MenuPanel
{
    [SerializeField] private Button _btnCreate;
    [SerializeField] private Button _btnOpenJoin;
    [SerializeField] private Button _btnBack;

    public event Action OnCreateClicked;
    public event Action OnOpenJoinClicked;
    public event Action OnBackClicked;

    private void Awake()
    {
        _btnCreate.onClick.AddListener(() => OnCreateClicked?.Invoke());
        _btnOpenJoin.onClick.AddListener(() => OnOpenJoinClicked?.Invoke());
        _btnBack.onClick.AddListener(() => OnBackClicked?.Invoke());
    }
}