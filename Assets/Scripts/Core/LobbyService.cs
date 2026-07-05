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
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyMemberJoined -= HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= HandleMemberLeft;
    }

    public async Task CreateLobby(int maxPlayers = 4)
    {
        var result = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);

        if (result.HasValue == false)
        {
            OnJoinFailed?.Invoke();
            return;
        }

        CurrentLobby = result.Value;
        CurrentLobby.SetPublic();
        CurrentLobby.SetJoinable(true);
        CurrentLobby.SetData("hostSteamId", SteamClient.SteamId.Value.ToString());
        IsHost = true;

        OnLobbyCreated?.Invoke();
    }

    public async Task JoinLobby(ulong lobbyId)
    {
        var result = await SteamMatchmaking.JoinLobbyAsync(lobbyId);

        if (result.HasValue == false)
        {
            OnJoinFailed?.Invoke();
            return;
        }

        CurrentLobby = result.Value;
        IsHost = false;

        OnLobbyJoined?.Invoke();
    }

    public void LeaveLobby()
    {
        CurrentLobby.Leave();
        IsHost = false;
        OnLobbyLeft?.Invoke();
    }

    public void CloseLobby()
    {
        if (IsHost == false)
            return;

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
    private void HandleMemberLeft(Lobby lobby, Friend friend) => OnMemberLeft?.Invoke(friend);
}