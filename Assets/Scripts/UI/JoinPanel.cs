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

    private void Awake()
    {
        _btnConfirm.onClick.AddListener(HandleConfirm);
        _btnBack.onClick.AddListener(() => OnBackClicked?.Invoke());
    }

    private void HandleConfirm()
    {
        if (ulong.TryParse(_lobbyIdInput.text, out ulong id) == false)
        {
            Debug.LogWarning("Invalid lobby ID");
            return;
        }

        OnJoinConfirmed?.Invoke(id);
    }
}