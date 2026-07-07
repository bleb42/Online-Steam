using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using FishNet;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;

    private void Start()
    {
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
    }

    private void OnDestroy()
    {
        if (InstanceFinder.ServerManager == null) return;

        InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        Debug.Log($"[DebugPlayerSpawner] Server state: {args.ConnectionState}");
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != RemoteConnectionState.Started)
            return;

        Vector3 position = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
        Quaternion rotation = _spawnPoint != null ? _spawnPoint.rotation : Quaternion.identity;

        NetworkObject player = Instantiate(_playerPrefab, position, rotation);
        InstanceFinder.ServerManager.Spawn(player, conn);

        Debug.Log($"[DebugPlayerSpawner] Spawned player for connection {conn.ClientId}");
    }
}
