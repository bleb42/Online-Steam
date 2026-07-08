using FishNet;
using UnityEngine;

public class DebugLocalNetworkStarter : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "Game";

    public void DebugStartHostLocal()
    {
        PlayerPrefs.SetInt("UseLocalTransport", 1);
        NetworkService.Instance.StartHost();

        NetworkService.Instance.OnConnected += OnHostConnected;
    }

    public void DebugJoinLocal()
    {
        PlayerPrefs.SetInt("UseLocalTransport", 1);
        NetworkService.Instance.StartClient(0);
    }

    private void OnHostConnected()
    {
        NetworkService.Instance.OnConnected -= OnHostConnected;
        NetworkService.Instance.BeginGameStart(_gameSceneName);
    }
}