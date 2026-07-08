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
        Debug.Log("[LobbyService] CreateLobby called");

        var result = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);

        if (!result.HasValue)
        {
            Debug.LogWarning("[LobbyService] CreateLobbyAsync returned null — failed to create lobby");
            OnJoinFailed?.Invoke();
            return;
        }

        CurrentLobby = result.Value;
        Debug.Log($"[LobbyService] Lobby created, Id={CurrentLobby.Id.Value}, Owner={CurrentLobby.Owner.Id.Value}, SteamId={SteamClient.SteamId.Value}");

        CurrentLobby.SetPublic();
        CurrentLobby.SetJoinable(true);
        CurrentLobby.SetData("hostSteamId", SteamClient.SteamId.Value.ToString());
        CurrentLobby.SetData("gameStarted", "false");
        IsHost = true;
        OnLobbyCreated?.Invoke();
    }

    public async Task JoinLobby(ulong lobbyId)
    {
        Debug.Log($"[LobbyService] JoinLobby called with id={lobbyId}, my SteamId={SteamClient.SteamId.Value}");

        var result = await SteamMatchmaking.JoinLobbyAsync(lobbyId);

        if (!result.HasValue)
        {
            Debug.LogWarning("[LobbyService] JoinLobbyAsync returned null — lobby not found or join failed");
            OnJoinFailed?.Invoke();
            return;
        }

        Debug.Log($"[LobbyService] Joined lobby raw result, Id={result.Value.Id.Value}, Owner={result.Value.Owner.Id.Value}, MemberCount={result.Value.MemberCount}");

        if (result.Value.GetData("gameStarted") == "true")
        {
            Debug.LogWarning("[LobbyService] gameStarted == true, leaving");
            result.Value.Leave();
            OnGameAlreadyStarted?.Invoke();
            return;
        }

        var owner = result.Value.Owner;

        if (owner.Id.Value == 0)
        {
            Debug.LogWarning("[LobbyService] owner.Id.Value == 0 — invalid owner, leaving");
            result.Value.Leave();
            OnJoinFailed?.Invoke();
            return;
        }

        var members = result.Value.Members.ToArray();
        Debug.Log($"[LobbyService] Members count = {members.Length}");

        //if (members.Length <= 1)
        //{
        //    Debug.LogWarning("[LobbyService] members.Length <= 1 — leaving");
        //    result.Value.Leave();
        //    OnJoinFailed?.Invoke();
        //    return;
        //}

        CurrentLobby = result.Value;
        IsHost = false;
        Debug.Log("[LobbyService] JoinLobby successful");
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
        if (!IsHost)
            return;

        try
        {
            CurrentLobby.SetData("gameStarted", "false");
            CurrentLobby.SetJoinable(false);
            CurrentLobby.Leave();
        }
        catch (Exception)
        {
            // Steam client already shutting down — safe to ignore here
        }
    }
}