using System;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using UnityEngine;

public class NetworkService : MonoBehaviour
{
    public static NetworkService Instance { get; private set; }

    public event Action OnConnected;
    public event Action OnDisconnected;

    private NetworkManager _networkManager;
    private Transport _transport;
    private Multipass _multipass;

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
    }

    private void OnDisable()
    {
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        _networkManager.ClientManager.UnregisterBroadcast<FadeOutMessage>(OnFadeOutReceived);
    }

    public void StartHost()
    {
        bool local = PlayerPrefs.GetInt("UseLocalTransport", 0) == 1;
        _multipass.SetClientTransport(local ? (Transport)_networkManager.TransportManager.GetTransport<Tugboat>() : _transport);
        _networkManager.ServerManager.StartConnection();
        _networkManager.ClientManager.StartConnection();
        _networkManager.ServerManager.OnServerConnectionState += OnServerReady;
    }

    public void StartClient(ulong hostSteamId)
    {
        bool local = PlayerPrefs.GetInt("UseLocalTransport", 0) == 1;

        if (local)
            _multipass.SetClientTransport<Tugboat>();
        else
        {
            _multipass.SetClientTransport<FishyFacepunch.FishyFacepunch>();
            _transport.SetClientAddress(hostSteamId.ToString());
        }

        _networkManager.ClientManager.StartConnection();
    }

    public void Disconnect()
    {
        _networkManager.ClientManager.StopConnection();
        _networkManager.ServerManager.StopConnection(true);
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            OnConnected?.Invoke();

            if (_networkManager.IsServerStarted == false)
                ScreenFader.Instance.FadeIn();
        }

        if (args.ConnectionState == LocalConnectionState.Stopped)
            OnDisconnected?.Invoke();
    }

    private void OnServerReady(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Started) 
            return;

        _networkManager.ServerManager.OnServerConnectionState -= OnServerReady;
        _networkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd;

        ScreenFader.Instance.FadeIn(() =>
        {
            var data = new FishNet.Managing.Scened.SceneLoadData("Game");
            data.ReplaceScenes = FishNet.Managing.Scened.ReplaceOption.All;
            _networkManager.SceneManager.LoadGlobalScenes(data);
        });
    }

    private void OnSceneLoadEnd(FishNet.Managing.Scened.SceneLoadEndEventArgs args)
    {
        if (!args.QueueData.AsServer) return;
        _networkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        _networkManager.ServerManager.Broadcast(new FadeOutMessage());
    }

    private void OnFadeOutReceived(FadeOutMessage msg, Channel channel)
    {
        ScreenFader.Instance.FadeOut();
    }
}

public struct FadeOutMessage : FishNet.Broadcast.IBroadcast { }