using UnityEngine;
using Steamworks;

public class SteamManager : PersistentSingleton<SteamManager>
{
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