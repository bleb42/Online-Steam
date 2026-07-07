using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MenuPanel
{
    [SerializeField] private TMP_Text _lobbyIdText;
    [SerializeField] private Transform _playersListParent;
    [SerializeField] private GameObject _playerEntryPrefab;
    [SerializeField] private Button _btnStartGame;
    [SerializeField] private Button _btnCopyId;
    [SerializeField] private Button _btnLeave;

    public event Action OnStartGameClicked;
    public event Action OnLeaveClicked;

    private void Awake()
    {
        _btnStartGame.onClick.AddListener(HandleStartGame);
        _btnCopyId.onClick.AddListener(() => GUIUtility.systemCopyBuffer = _lobbyIdText.text);
        _btnLeave.onClick.AddListener(() => OnLeaveClicked?.Invoke());
    }

    private void HandleStartGame()
    {
        _btnStartGame.interactable = false;
        OnStartGameClicked?.Invoke();
    }

    public void ShowStartButton(bool show) => _btnStartGame.gameObject.SetActive(show);

    public void SetLobbyId(string id) => _lobbyIdText.text = id;

    public void RefreshPlayers(string[] playerNames)
    {
        foreach (Transform child in _playersListParent)
            Destroy(child.gameObject);

        foreach (var name in playerNames)
        {
            var entry = Instantiate(_playerEntryPrefab, _playersListParent);
            entry.GetComponentInChildren<TMP_Text>().text = name;
        }
    }
}