using System;
using System.Collections;
using System.Diagnostics;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

public class NetworkService : MonoBehaviour
{
    public static NetworkService Instance { get; private set; }

    public event Action OnConnected;
    public event Action OnDisconnected;

    private NetworkManager _networkManager;
    private Transport _transport;

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
    }

    private void OnEnable()
    {
        _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDisable()
    {
        _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    public void StartHost()
    {
        _networkManager.ServerManager.StartConnection();
        _networkManager.ClientManager.StartConnection();
    }

    public void StartClient(ulong hostSteamId)
    {
        _transport.SetClientAddress(hostSteamId.ToString());
        _networkManager.ClientManager.StartConnection();
    }

    public void Disconnect()
    {
        _networkManager.ClientManager.StopConnection();
        _networkManager.ServerManager.StopConnection(true);
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        UnityEngine.Debug.Log($"Server state: {args.ConnectionState}");

        if (args.ConnectionState == LocalConnectionState.Started)
        {
            UnityEngine.Debug.Log("Starting LoadGameScene coroutine");
            OnConnected?.Invoke();
            StartCoroutine(LoadGameScene());
            _networkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd;
        }
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        UnityEngine.Debug.Log($"Client state: {args.ConnectionState}");

        if (args.ConnectionState == LocalConnectionState.Started)
            OnConnected?.Invoke();

        if (args.ConnectionState == LocalConnectionState.Stopped)
            OnDisconnected?.Invoke();
    }

    private IEnumerator LoadGameScene()
    {
        ScreenFader.Instance.AutoFadeOut = false;
        ScreenFader.Instance.FadeIn();

        yield return new WaitForSeconds(ScreenFader.Instance.Duration);

        var sceneLoadData = new FishNet.Managing.Scened.SceneLoadData("Game");
        sceneLoadData.ReplaceScenes = FishNet.Managing.Scened.ReplaceOption.All;
        _networkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
    }

    private void OnSceneLoadEnd(FishNet.Managing.Scened.SceneLoadEndEventArgs args)
    {
        if (args.QueueData.AsServer == false) 
            return;

        _networkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        ScreenFader.Instance.AutoFadeOut = true;
        ScreenFader.Instance.FadeOut();
    }
}