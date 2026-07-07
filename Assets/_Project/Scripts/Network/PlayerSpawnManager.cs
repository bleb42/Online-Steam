using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager Instance { get; private set; }

    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    private int _expectedPlayers;
    private int _readyPlayers;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnAllPlayers(IEnumerable<NetworkConnection> connections)
    {
        if (!InstanceFinder.IsServerStarted)
            return;

        _readyPlayers = 0;
        _expectedPlayers = 0;

        foreach (var conn in connections)
        {
            _expectedPlayers++;
            Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            NetworkObject player = Instantiate(_playerPrefab, point.position, point.rotation);
            InstanceFinder.ServerManager.Spawn(player, conn);
        }
    }

    public void NotifyPlayerReady()
    {
        _readyPlayers++;
        Debug.Log($"[PlayerSpawnManager] Ready {_readyPlayers}/{_expectedPlayers}");

        if (_readyPlayers >= _expectedPlayers)
        {
            if (NetworkService.Instance != null)
            {
                NetworkService.Instance.FinishGameStart();
            }
            else
            {
                Debug.Log("[PlayerSpawnManager] NetworkService not present (debug scene) — skipping fade out.");
            }
        }
    }
}