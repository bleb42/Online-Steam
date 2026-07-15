using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class GameSettingsService : PersistentSingleton<GameSettingsService>
{
    [SerializeField] private AudioMixer _audioMixer;

    public GameSettingsData Current { get; private set; }

    public event Action OnSettingsChanged;

    private string SavePath => Path.Combine(Application.persistentDataPath, SettingsConstants.Files.SettingsFileName);

    protected override void Awake()
    {
        base.Awake();

        Load();
        Apply();
    }

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                Current = JsonUtility.FromJson<GameSettingsData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameSettingsService] Failed to load settings, using defaults. {e}");
                Current = CreateDefault();
            }
        }
        else
        {
            Current = CreateDefault();
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(Current, true);
        File.WriteAllText(SavePath, json);
    }

    public void Apply()
    {
        Screen.SetResolution(Current.ResolutionWidth, Current.ResolutionHeight, (FullScreenMode)Current.FullScreenMode);
        QualitySettings.vSyncCount = Current.VSync ? 1 : 0;

        if (_audioMixer != null)
        {
            SetMixerVolume(SettingsConstants.AudioMixerParams.Master, Current.MasterVolume);
            SetMixerVolume(SettingsConstants.AudioMixerParams.Music, Current.MusicVolume);
            SetMixerVolume(SettingsConstants.AudioMixerParams.Sfx, Current.SfxVolume);
        }

        OnSettingsChanged?.Invoke();
    }

    public void SetResolution(int width, int height, FullScreenMode mode)
    {
        Current.ResolutionWidth = width;
        Current.ResolutionHeight = height;
        Current.FullScreenMode = (int)mode;
        Apply();
        Save();
    }

    public void SetVSync(bool enabled)
    {
        Current.VSync = enabled;
        Apply();
        Save();
    }

    public void SetVolume(VolumeChannel channel, float value)
    {
        switch (channel)
        {
            case VolumeChannel.Master: Current.MasterVolume = value; break;
            case VolumeChannel.Music: Current.MusicVolume = value; break;
            case VolumeChannel.Sfx: Current.SfxVolume = value; break;
        }
        Apply();
        Save();
    }

    public void SetMouseSensitivity(float value)
    {
        Current.MouseSensitivity = value;
        OnSettingsChanged?.Invoke();
        Save();
    }

    public void SetInvertCamera(bool value)
    {
        Current.InvertCamera = value;
        OnSettingsChanged?.Invoke();
        Save();
    }

    private GameSettingsData CreateDefault()
    {
        return new GameSettingsData
        {
            ResolutionWidth = Screen.currentResolution.width,
            ResolutionHeight = Screen.currentResolution.height,
            FullScreenMode = (int)Screen.fullScreenMode
        };
    }

    private void SetMixerVolume(string exposedParam, float linearVolume)
    {
        float dB = linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20f : -80f;
        _audioMixer.SetFloat(exposedParam, dB);
    }
}