using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoSettingsPanel : MonoBehaviour
{
    private static readonly FullScreenMode[] FullscreenModes =
    {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed
    };

    private static readonly string[] FullscreenModeLabels =
    {
        SettingsConstants.UiText.FullscreenExclusive,
        SettingsConstants.UiText.FullscreenBorderless,
        SettingsConstants.UiText.FullscreenWindowed
    };

    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Dropdown _fullscreenModeDropdown;
    [SerializeField] private Toggle _vsyncToggle;

    private Resolution[] _resolutions;

    private void OnEnable()
    {
        SetupResolutions();
        SetupFullscreenMode();
        SetupVSync();
    }

    private void SetupResolutions()
    {
        _resolutions = Screen.resolutions
            .Select(r => new Resolution { width = r.width, height = r.height })
            .GroupBy(r => (r.width, r.height))
            .Select(g => g.First())
            .OrderBy(r => r.width * r.height)
            .ToArray();

        _resolutionDropdown.ClearOptions();
        _resolutionDropdown.AddOptions(_resolutions.Select(r => $"{r.width} x {r.height}").ToList());

        var current = GameSettingsService.Instance.Current;
        int index = System.Array.FindIndex(_resolutions, r => r.width == current.ResolutionWidth && r.height == current.ResolutionHeight);
        _resolutionDropdown.SetValueWithoutNotify(index >= 0 ? index : _resolutions.Length - 1);

        _resolutionDropdown.onValueChanged.RemoveAllListeners();
        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void SetupFullscreenMode()
    {
        _fullscreenModeDropdown.ClearOptions();
        _fullscreenModeDropdown.AddOptions(new List<string>(FullscreenModeLabels));

        var mode = (FullScreenMode)GameSettingsService.Instance.Current.FullScreenMode;
        int index = System.Array.IndexOf(FullscreenModes, mode);
        _fullscreenModeDropdown.SetValueWithoutNotify(index >= 0 ? index : FullscreenModes.Length - 1);

        _fullscreenModeDropdown.onValueChanged.RemoveAllListeners();
        _fullscreenModeDropdown.onValueChanged.AddListener(OnFullscreenModeChanged);
    }

    private void SetupVSync()
    {
        _vsyncToggle.SetIsOnWithoutNotify(GameSettingsService.Instance.Current.VSync);
        _vsyncToggle.onValueChanged.RemoveAllListeners();
        _vsyncToggle.onValueChanged.AddListener(v => GameSettingsService.Instance.SetVSync(v));
    }

    private void OnResolutionChanged(int index)
    {
        var res = _resolutions[index];
        var mode = (FullScreenMode)GameSettingsService.Instance.Current.FullScreenMode;
        GameSettingsService.Instance.SetResolution(res.width, res.height, mode);
    }

    private void OnFullscreenModeChanged(int index)
    {
        var mode = FullscreenModes[index];
        var current = GameSettingsService.Instance.Current;
        GameSettingsService.Instance.SetResolution(current.ResolutionWidth, current.ResolutionHeight, mode);
    }
}