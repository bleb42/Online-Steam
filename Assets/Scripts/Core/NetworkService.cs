using System;
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
        Debug.Log($"Server state: {args.ConnectionState}");

        if (args.ConnectionState == LocalConnectionState.Started)
        {
            OnConnected?.Invoke();
            ScreenFader.Instance.FadeIn(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
            });
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
}