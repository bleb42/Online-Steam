using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebindService : PersistentSingleton<InputRebindService>
{
    public event Action OnRebindsChanged;

    private Controls _templateControls;

    protected override void Awake()
    {
        base.Awake();

        _templateControls = new Controls();
        LoadIntoAsset(_templateControls.asset);
    }

    public InputAction GetAction(string actionName)
    {
        return _templateControls.asset.FindAction(actionName);
    }

    public void SaveCurrentBindings()
    {
        string json = _templateControls.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(SettingsConstants.PlayerPrefsKeys.InputRebinds, json);
        PlayerPrefs.Save();
        OnRebindsChanged?.Invoke();
    }

    public void LoadIntoAsset(InputActionAsset asset)
    {
        if (PlayerPrefs.HasKey(SettingsConstants.PlayerPrefsKeys.InputRebinds))
        {
            string json = PlayerPrefs.GetString(SettingsConstants.PlayerPrefsKeys.InputRebinds);
            asset.LoadBindingOverridesFromJson(json);
        }
    }

    public void ResetToDefaults()
    {
        _templateControls.asset.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(SettingsConstants.PlayerPrefsKeys.InputRebinds);
        OnRebindsChanged?.Invoke();
    }
}