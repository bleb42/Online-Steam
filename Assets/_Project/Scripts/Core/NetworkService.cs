using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using UnityEngine;

public class NetworkService : PersistentSingleton<NetworkService>
{
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<int> OnClientCountChanged;
    public event Action<IEnumerable<NetworkConnection>> OnGameSceneReadyToSpawn;
    public event Action OnPlayerReadyOnServer;

    private NetworkManager _networkManager;
    private Transport _transport;

    private bool _connectionStarted;

    protected override void Awake()
    {
        base.Awake();

        _networkManager = InstanceFinder.NetworkManager;
        _transport = _networkManager.TransportManager.GetTransport<FishyFacepunch.FishyFacepunch>();

        Debug.Log($"Steam initialized: {Steamworks.SteamClient.IsValid}");

        if (Steamworks.SteamClient.IsValid)
            Debug.Log($"SteamId: {Steamworks.SteamClient.SteamId}");
    }

    private void OnEnable()
    {
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        _networkManager.ClientManager.RegisterBroadcast<FadeOutMessage>(OnFadeOutReceived);
        _networkManager.ClientManager.RegisterBroadcast<FadeInMessage>(OnFadeInReceived);
        _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionStateLog;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateLog;
    }

    private void OnDisable()
    {
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        _networkManager.ClientManager.UnregisterBroadcast<FadeOutMessage>(OnFadeOutReceived);
        _networkManager.ClientManager.UnregisterBroadcast<FadeInMessage>(OnFadeInReceived);
        _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;

        _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionStateLog;
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionStateLog;
    }

    public int GetConnectedClientCount() => _networkManager.ServerManager.Clients.Count;

    public void StartHost()
    {
        Debug.Log("[NetworkService] StartHost, transport = FishyFacepunch");

        _networkManager.ServerManager.StartConnection();
        _networkManager.ClientManager.StartConnection();

        _connectionStarted = true;
    }

    public void StartClient(ulong hostSteamId)
    {
        Debug.Log($"[NetworkService] StartClient SteamId={hostSteamId}");

        _transport.SetClientAddress(hostSteamId.ToString());
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

    public void NotifyPlayerReady()
    {
        OnPlayerReadyOnServer?.Invoke();
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

        OnGameSceneReadyToSpawn?.Invoke(_networkManager.ServerManager.Clients.Values);
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

    private void OnServerConnectionStateLog(ServerConnectionStateArgs args)
    {
        Debug.Log($"[Server] state={args.ConnectionState}");
    }

    private void OnClientConnectionStateLog(ClientConnectionStateArgs args)
    {
        Debug.Log($"[Client] state={args.ConnectionState}");
    }
}

public struct FadeOutMessage : FishNet.Broadcast.IBroadcast { }
public struct FadeInMessage : FishNet.Broadcast.IBroadcast { }