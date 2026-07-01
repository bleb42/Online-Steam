using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _panelMain;
    [SerializeField] private GameObject _panelMultiplayer;
    [SerializeField] private GameObject _panelJoin;
    [SerializeField] private GameObject _panelLobby;
    [SerializeField] private GameObject _panelLoading;

    [Header("Buttons")]
    [SerializeField] private Button _btnMultiplayer;
    [SerializeField] private Button _btnCreate;
    [SerializeField] private Button _btnOpenJoin;
    [SerializeField] private Button _btnConfirmJoin;
    [SerializeField] private Button _btnBackFromMultiplayer;
    [SerializeField] private Button _btnBackFromJoin;
    [SerializeField] private Button _btnStartGame;
    [SerializeField] private Button _btnCopyId;
    [SerializeField] private Button _btnLeave;

    [Header("Join Panel")]
    [SerializeField] private TMP_InputField _lobbyIdInput;

    [Header("Lobby Panel")]
    [SerializeField] private TMP_Text _lobbyIdText;
    [SerializeField] private Transform _playersListParent;
    [SerializeField] private GameObject _playerEntryPrefab;

    private GameObject[] _allPanels;

    private void Awake()
    {
        _allPanels = new[] { _panelMain, _panelMultiplayer, _panelJoin, _panelLobby, _panelLoading };

        _btnMultiplayer.onClick.AddListener(OnClickMultiplayer);
        _btnCreate.onClick.AddListener(OnClickCreate);
        _btnOpenJoin.onClick.AddListener(OnClickOpenJoin);
        _btnConfirmJoin.onClick.AddListener(OnClickJoin);
        _btnBackFromMultiplayer.onClick.AddListener(OnClickBack);
        _btnBackFromJoin.onClick.AddListener(OnClickBackToMultiplayer);
        _btnStartGame.onClick.AddListener(OnClickStartGame);
        _btnCopyId.onClick.AddListener(OnClickCopyId);
        _btnLeave.onClick.AddListener(OnClickLeave);
    }

    private void Start()
    {
        ShowPanel(_panelMain);
        _btnStartGame.gameObject.SetActive(false);

        LobbyService.Instance.OnLobbyCreated += HandleLobbyCreated;
        LobbyService.Instance.OnLobbyJoined += HandleLobbyJoined;
        LobbyService.Instance.OnLobbyLeft += HandleLobbyLeft;
        LobbyService.Instance.OnMemberJoined += _ => RefreshPlayersList();
        LobbyService.Instance.OnMemberLeft += _ => RefreshPlayersList();
        LobbyService.Instance.OnJoinFailed += HandleJoinFailed;

        LobbyService.Instance.OnGameStarted += HandleGameStarted;
    }

    private void OnDestroy()
    {
        if (LobbyService.Instance == null) return;

        LobbyService.Instance.OnLobbyCreated -= HandleLobbyCreated;
        LobbyService.Instance.OnLobbyJoined -= HandleLobbyJoined;
        LobbyService.Instance.OnLobbyLeft -= HandleLobbyLeft;
        LobbyService.Instance.OnJoinFailed -= HandleJoinFailed;
    }

    private void HandleGameStarted()
    {
        ulong hostId = LobbyService.Instance.GetHostSteamId();
        NetworkService.Instance.StartClient(hostId);
    }

    private void OnClickMultiplayer() => ShowPanel(_panelMultiplayer);

    private void OnClickOpenJoin() => ShowPanel(_panelJoin);

    private void OnClickBack() => ShowPanel(_panelMain);

    private void OnClickBackToMultiplayer() => ShowPanel(_panelMultiplayer);

    private void OnClickCopyId() => GUIUtility.systemCopyBuffer = LobbyService.Instance.GetLobbyId();

    private void OnClickLeave() => LobbyService.Instance.LeaveLobby();

    private void OnClickStartGame()
    {
        LobbyService.Instance.StartGame();
        NetworkService.Instance.StartHost();
    }

    private async void OnClickCreate()
    {
        ShowPanel(_panelLoading);
        await LobbyService.Instance.CreateLobby();
    }

    private async void OnClickJoin()
    {
        if (ulong.TryParse(_lobbyIdInput.text, out ulong id) == false)
        {
            Debug.LogWarning("Invalid lobby ID");
            return;
        }

        ShowPanel(_panelLoading);
        await LobbyService.Instance.JoinLobby(id);
    }

    private void HandleLobbyCreated()
    {
        ShowPanel(_panelLobby);
        _lobbyIdText.text = LobbyService.Instance.GetLobbyId();
        _btnStartGame.gameObject.SetActive(true);
        RefreshPlayersList();
    }

    private void HandleLobbyJoined()
    {
        ShowPanel(_panelLobby);
        _lobbyIdText.text = LobbyService.Instance.GetLobbyId();
        _btnStartGame.gameObject.SetActive(false);
        RefreshPlayersList();
    }

    private void HandleLobbyLeft() => ShowPanel(_panelMain);

    private void HandleJoinFailed()
    {
        ShowPanel(_panelMultiplayer);
        Debug.LogWarning("Join failed");
    }

    private void RefreshPlayersList()
    {
        foreach (Transform child in _playersListParent)
            Destroy(child.gameObject);

        foreach (var member in LobbyService.Instance.GetMembers())
        {
            var entry = Instantiate(_playerEntryPrefab, _playersListParent);
            entry.GetComponentInChildren<TMP_Text>().text = member.Name;
        }
    }

    private void ShowPanel(GameObject panel)
    {
        foreach (var p in _allPanels)
            p.SetActive(false);

        panel.SetActive(true);
    }
}