using FishNet;
using FishNet.Object;
using UnityEngine;

public class PlayerNetworkSetup : NetworkBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private AudioListener _audioListener;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _inputReader.enabled = IsOwner;
        _cameraController.enabled = IsOwner;

        if (_playerCamera != null)
            _playerCamera.gameObject.SetActive(IsOwner);
        if (_audioListener != null)
            _audioListener.enabled = IsOwner;

        if (IsOwner)
        {
            NotifyReadyServerRpc();
        }
    }

    [ServerRpc]
    private void NotifyReadyServerRpc()
    {
        PlayerSpawnManager.Instance.NotifyPlayerReady();
    }
}