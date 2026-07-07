using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinPanel : MenuPanel
{
    [SerializeField] private TMP_InputField _lobbyIdInput;
    [SerializeField] private Button _btnConfirm;
    [SerializeField] private Button _btnBack;

    public event Action<ulong> OnJoinConfirmed;
    public event Action OnBackClicked;
    public event Action OnInvalidId;

    private void Awake()
    {
        _btnConfirm.onClick.AddListener(HandleConfirm);
        _btnBack.onClick.AddListener(() => OnBackClicked?.Invoke());
    }

    public override void Show()
    {
        base.Show();
        _lobbyIdInput.text = "";
    }

    private void HandleConfirm()
    {
        if (ulong.TryParse(_lobbyIdInput.text, out ulong id) == false)
        {
            OnInvalidId?.Invoke();
            return;
        }

        OnJoinConfirmed?.Invoke(id);
    }
}