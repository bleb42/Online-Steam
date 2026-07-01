using UnityEngine;
using Steamworks;
using System;

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

        try
        {
            SteamClient.Init(480);
            Debug.Log($"Steam initialized: {SteamClient.Name}");
        }
        catch (Exception e)
        { 
            Debug.Log(e.Message); 
            Application.Quit();
        }
    }

    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        SteamClient.Shutdown();
    }
}