using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MenuPanel
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _btnOk;

    public event Action OnOkClicked;

    private void Awake()
    {
        _btnOk.onClick.AddListener(() => OnOkClicked?.Invoke());
    }

    public void SetMessage(string title, string message)
    {
        _titleText.text = title;
        _messageText.text = message;
    }
}