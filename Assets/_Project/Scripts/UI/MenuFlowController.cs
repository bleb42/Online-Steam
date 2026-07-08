using System.Linq;
using UnityEngine;

public class MenuFlowController : MonoBehaviour
{
    private const string GameSceneHash = "Game";

    [SerializeField] private PanelRouter _router;

    [SerializeField] private MainMenuPanel _mainMenu;
    [SerializeField] private MultiplayerPanel _multiplayer;
    [SerializeField] private JoinPanel _join;
    [SerializeField] private LobbyPanel _lobby;
    [SerializeField] private LoadingPanel _loading;
    [SerializeField] private MessagePanel _message;
    [SerializeField] private SettingsPanel _settings;

    private void Awake()
    {
        _mainMenu.OnMultiplayerClicked += () => _router.Show(_multiplayer);
        _mainMenu.OnSettingsClicked += () => _router.Show(_settings);

        _multiplayer.OnCreateClicked += HandleCreateClicked;
        _multiplayer.OnOpenJoinClicked += () => _router.Show(_join);
        _multiplayer.OnBackClicked += () => _router.Show(_mainMenu);

        _join.OnJoinConfirmed += HandleJoinConfirmed;
        _join.OnBackClicked += () => _router.Show(_multiplayer);
        _join.OnInvalidId += HandleInvalidId;

        _lobby.OnStartGameClicked += HandleStartGameClicked;
        _lobby.OnLeaveClicked += HandleLeaveClicked;

        _message.OnOkClicked += () => _router.Show(_mainMenu);

        _settings.OnBackClicked += () => _router.Show(_mainMenu);
    }

    private void Start()
    {
        LobbyService.Instance.OnLobbyCreated += HandleLobbyCreated;
        LobbyService.Instance.OnLobbyJoined += HandleLobbyJoined;
        LobbyService.Instance.OnLobbyLeft += HandleLobbyLeft;
        LobbyService.Instance.OnMemberJoined += _ => RefreshPlayersList();
        LobbyService.Instance.OnMemberLeft += _ => RefreshPlayersList();
        LobbyService.Instance.OnJoinFailed += HandleJoinFailed;
        LobbyService.Instance.OnHostLeft += HandleHostLeft;
        LobbyService.Instance.OnGameAlreadyStarted += HandleGameAlreadyStarted;
    }

    private void OnDestroy()
    {
        if (LobbyService.Instance == null) 
            return;

        LobbyService.Instance.OnLobbyCreated -= HandleLobbyCreated;
        LobbyService.Instance.OnLobbyJoined -= HandleLobbyJoined;
        LobbyService.Instance.OnLobbyLeft -= HandleLobbyLeft;
        LobbyService.Instance.OnJoinFailed -= HandleJoinFailed;
        LobbyService.Instance.OnHostLeft -= HandleHostLeft;
        LobbyService.Instance.OnGameAlreadyStarted -= HandleGameAlreadyStarted;
    }

    private async void HandleCreateClicked()
    {
        _router.Show(_loading);
        await LobbyService.Instance.CreateLobby();
    }

    private async void HandleJoinConfirmed(ulong id)
    {
        _router.Show(_loading);
        await LobbyService.Instance.JoinLobby(id);
    }

    private void HandleStartGameClicked()
    {
        LobbyService.Instance.CloseLobby();
        NetworkService.Instance.BeginGameStart(GameSceneHash);
    }

    private void HandleLeaveClicked()
    {
        NetworkService.Instance.Disconnect();
        LobbyService.Instance.LeaveLobby();
    }

    private void HandleLobbyCreated()
    {
        _router.Show(_lobby);
        _lobby.SetLobbyId(LobbyService.Instance.GetLobbyId());
        _lobby.ShowStartButton(true);
        RefreshPlayersList();

        NetworkService.Instance.StartHost();
    }

    private void HandleLobbyJoined()
    {
        _router.Show(_lobby);
        _lobby.SetLobbyId(LobbyService.Instance.GetLobbyId());
        _lobby.ShowStartButton(false);
        RefreshPlayersList();

        ulong hostId = LobbyService.Instance.GetHostSteamId();
        NetworkService.Instance.StartClient(hostId);
    }

    private void HandleLobbyLeft() => _router.Show(_mainMenu);

    private void HandleJoinFailed()
    {
        _message.SetMessage("Lobby not found", "This lobby no longer exists.");
        _router.Show(_message);
    }

    private void HandleGameAlreadyStarted()
    {
        _message.SetMessage("Game in progress", "This game has already started.");
        _router.Show(_message);
    }

    private void HandleInvalidId()
    {
        _message.SetMessage("Invalid ID", "Please enter a valid lobby ID.");
        _router.Show(_message);
    }

    private void HandleHostLeft()
    {
        NetworkService.Instance.Disconnect();
        LobbyService.Instance.LeaveLobby();

        _message.SetMessage("Lobby closed", "The host has left the lobby.");
        _router.Show(_message);
    }

    private void RefreshPlayersList()
    {
        var names = LobbyService.Instance.GetMembers()
            .Select(f => f.Name)
            .ToArray();

        _lobby.RefreshPlayers(names);
    }
}