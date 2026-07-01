using System;
using System.Collections;
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

    private Coroutine _switchingScene;

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
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            OnConnected?.Invoke();

            if (_switchingScene != null)
                return;

            _switchingScene = StartCoroutine(LoadGameScene());
        }
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        Debug.Log($"Client state: {args.ConnectionState}");

        if (args.ConnectionState == LocalConnectionState.Started)
            OnConnected?.Invoke();

        if (args.ConnectionState == LocalConnectionState.Stopped)
            OnDisconnected?.Invoke();
    }

    private IEnumerator LoadGameScene()
    {
        ScreenFader.Instance.FadeIn(() => { });
        yield return new WaitForSeconds(ScreenFader.Instance.Duration);

        var sceneLoadData = new FishNet.Managing.Scened.SceneLoadData("Game");
        _networkManager.SceneManager.LoadGlobalScenes(sceneLoadData);

        yield return new WaitUntil(() =>
            UnityEngine.SceneManagement.SceneManager.GetSceneByName("Game").isLoaded);

        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Menu");

        ScreenFader.Instance.FadeOut();
    }
}