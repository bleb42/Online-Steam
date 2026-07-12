using System;
using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using UnityEngine;

public class NetworkService : MonoBehaviour
{
    public static NetworkService Instance { get; private set; }

    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<int> OnClientCountChanged;

    private NetworkManager _networkManager;
    private Transport _transport;
    private Multipass _multipass;

    private bool _connectionStarted;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _networkManager = InstanceFinder.NetworkManager;
        _transport = _networkManager.TransportManager.GetTransport<FishyFacepunch.FishyFacepunch>();
        _multipass = _networkManager.TransportManager.GetTransport<Multipass>();
    }

    private void OnEnable()
    {
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        _networkManager.ClientManager.RegisterBroadcast<FadeOutMessage>(OnFadeOutReceived);
        _networkManager.ClientManager.RegisterBroadcast<FadeInMessage>(OnFadeInReceived);
        _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    private void OnDisable()
    {
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        _networkManager.ClientManager.UnregisterBroadcast<FadeOutMessage>(OnFadeOutReceived);
        _networkManager.ClientManager.UnregisterBroadcast<FadeInMessage>(OnFadeInReceived);
        _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    public int GetConnectedClientCount() => _networkManager.ServerManager.Clients.Count;

    public void StartHost()
    {
        bool local = PlayerPrefs.GetInt("UseLocalTransport", 0) == 1;
        Debug.Log($"[NetworkService] StartHost, transport = {(local ? "Tugboat" : "FishyFacepunch")}");

        _multipass.SetClientTransport(local ? (Transport)_networkManager.TransportManager.GetTransport<Tugboat>() : _transport);
        _networkManager.ServerManager.StartConnection();
        _networkManager.ClientManager.StartConnection();
        _connectionStarted = true;
    }


    public void StartClient(ulong hostSteamId)
    {
        bool local = PlayerPrefs.GetInt("UseLocalTransport", 0) == 1;

        if (local)
        {
            _multipass.SetClientTransport<Tugboat>();
        }
        else
        {
            _multipass.SetClientTransport<FishyFacepunch.FishyFacepunch>();
            _transport.SetClientAddress(hostSteamId.ToString());
        }

        _networkManager.ClientManager.StartConnection();
        _connectionStarted = true;
    }

    public void Disconnect()
    {
        if (!_connectionStarted)
            return;

        _networkManager.ClientManager.StopConnection();
        _networkManager.ServerManager.StopConnection(true);
    }

    public void BeginGameStart(string sceneName)
    {
        _networkManager.ServerManager.Broadcast(new FadeInMessage(), true, Channel.Reliable);

        ScreenFader.Instance.FadeIn(() =>
        {
            _networkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd;

            var data = new SceneLoadData(sceneName);
            data.ReplaceScenes = ReplaceOption.All;
            _networkManager.SceneManager.LoadGlobalScenes(data);
        });
    }

    public void FinishGameStart()
    {
        Debug.Log("[NetworkService] FinishGameStart called - broadcasting FadeOut");
        _networkManager.ServerManager.Broadcast(new FadeOutMessage(), true, Channel.Reliable);
        Debug.Log("[NetworkService] FadeOut broadcast sent");
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
            OnConnected?.Invoke();

        if (args.ConnectionState == LocalConnectionState.Stopped)
            OnDisconnected?.Invoke();
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        if (args.QueueData.AsServer == false)
            return;

        _networkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;

        PlayerSpawnManager.Instance.SpawnAllPlayers(_networkManager.ServerManager.Clients.Values);
    }

    private void OnFadeOutReceived(FadeOutMessage msg, Channel channel)
    {
        Debug.Log("[NetworkService] OnFadeOutReceived - starting fade out");
        ScreenFader.Instance.FadeOut();
    }

    private void OnFadeInReceived(FadeInMessage msg, Channel channel)
    {
        if (InstanceFinder.IsServerStarted) 
            return;

        ScreenFader.Instance.FadeIn();
    }


    private void OnRemoteConnectionState(FishNet.Connection.NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        OnClientCountChanged?.Invoke(_networkManager.ServerManager.Clients.Count);
    }
}

public struct FadeOutMessage : FishNet.Broadcast.IBroadcast { }
public struct FadeInMessage : FishNet.Broadcast.IBroadcast { }