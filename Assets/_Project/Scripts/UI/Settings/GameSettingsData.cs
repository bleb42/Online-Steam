using System;

[Serializable]
public class GameSettingsData
{
    public int SchemaVersion = 1;

    // Video
    public int ResolutionWidth;
    public int ResolutionHeight;
    public int FullScreenMode;
    public bool VSync = true;

    // Audio
    public float MasterVolume = 1f;
    public float MusicVolume = 1f;
    public float SfxVolume = 1f;

    // Controls
    public float MouseSensitivity = 5f;
    public bool InvertCamera = false;
}