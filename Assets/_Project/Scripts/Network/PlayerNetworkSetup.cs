using FishNet.Object;
using UnityEngine;

public class PlayerNetworkSetup : NetworkBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private AudioListener _audioListener;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _inputReader.enabled = IsOwner;
        _cameraController.enabled = IsOwner;
        _playerController.enabled = IsOwner;

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
        if (PlayerSpawnManager.Instance != null)
        {
            PlayerSpawnManager.Instance.NotifyPlayerReady();
        }
        else
        {
            Debug.Log("[PlayerNetworkSetup] PlayerSpawnManager not present (debug scene) — skipping ready notification.");
        }
    }
}