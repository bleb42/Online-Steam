using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        if (NetworkService.Instance != null)
            NetworkService.Instance.Disconnect();

        SteamClient.Shutdown();
    }
}