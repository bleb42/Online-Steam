using System;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class LobbyService : MonoBehaviour
{
    public static LobbyService Instance { get; private set; }
    public Lobby CurrentLobby { get; private set; }
    public bool IsHost { get; private set; }

    public event Action OnLobbyCreated;
    public event Action OnLobbyJoined;
    public event Action OnLobbyLeft;
    public event Action<Friend> OnMemberJoined;
    public event Action<Friend> OnMemberLeft;
    public event Action OnJoinFailed;
    public event Action OnHostLeft;
    public event Action OnGameAlreadyStarted;
    public event Action OnGameStarted;

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
        SteamMatchmaking.OnLobbyMemberJoined += HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += HandleMemberLeft;
        SteamMatchmaking.OnLobbyDataChanged += HandleLobbyDataChanged;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyMemberJoined -= HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= HandleMemberLeft;
        SteamMatchmaking.OnLobbyDataChanged -= HandleLobbyDataChanged;
    }

    public async Task CreateLobby(int maxPlayers = 4)
    {
        var result = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);

        if (!result.HasValue) 
        { 
            OnJoinFailed?.Invoke(); 
            return; 
        }

        CurrentLobby = result.Value;
        CurrentLobby.SetPublic();
        CurrentLobby.SetJoinable(true);
        CurrentLobby.SetData("hostSteamId", SteamClient.SteamId.Value.ToString());
        CurrentLobby.SetData("gameStarted", "false");
        IsHost = true;
        OnLobbyCreated?.Invoke();
    }

    public async Task JoinLobby(ulong lobbyId)
    {
        var result = await SteamMatchmaking.JoinLobbyAsync(lobbyId);

        if (!result.HasValue) 
        { 
            OnJoinFailed?.Invoke(); 
            return; 
        }

        if (result.Value.GetData("gameStarted") == "true")
        {
            result.Value.Leave();
            OnGameAlreadyStarted?.Invoke();
            return;
        }

        var owner = result.Value.Owner;

        if (owner.Id.Value == 0)
        {
            result.Value.Leave();
            OnJoinFailed?.Invoke();
            return;
        }

        var members = result.Value.Members.ToArray();

        if (members.Length <= 1)
        {
            result.Value.Leave();
            OnJoinFailed?.Invoke();
            return;
        }

        CurrentLobby = result.Value;
        IsHost = false;
        OnLobbyJoined?.Invoke();
    }

    public void LeaveLobby()
    {
        if (IsHost)
            CurrentLobby.SetJoinable(false);

        CurrentLobby.Leave();
        IsHost = false;
        OnLobbyLeft?.Invoke();
    }

    public void CloseLobby()
    {
        if (!IsHost) 
            return;

        CurrentLobby.SetData("gameStarted", "true");
        CurrentLobby.SetJoinable(false);
    }

    public ulong GetHostSteamId()
    {
        string value = CurrentLobby.GetData("hostSteamId");
        return ulong.TryParse(value, out ulong id) ? id : 0;
    }

    public string GetLobbyId() => CurrentLobby.Id.Value.ToString();
    public Friend[] GetMembers() => CurrentLobby.Members.ToArray();

    private void HandleMemberJoined(Lobby lobby, Friend friend) => OnMemberJoined?.Invoke(friend);

    private void HandleMemberLeft(Lobby lobby, Friend friend)
    {
        OnMemberLeft?.Invoke(friend);

        if (IsHost) 
            return;

        if (friend.Id.Value == GetHostSteamId())
            OnHostLeft?.Invoke();
    }

    private void HandleLobbyDataChanged(Lobby lobby)
    {
        if (IsHost)
            return;

        if (lobby.GetData("gameStarted") == "true")
            OnGameStarted?.Invoke();
    }

    private void OnApplicationQuit()
    {
        if (IsHost)
        {
            CurrentLobby.SetData("gameStarted", "false");
            CurrentLobby.SetJoinable(false);
            CurrentLobby.Leave();
        }
    }
}