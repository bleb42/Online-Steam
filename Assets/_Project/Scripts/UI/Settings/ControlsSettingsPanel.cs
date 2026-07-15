using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ControlsSettingsPanel : MonoBehaviour
{
    [Serializable]
    private class RebindEntry
    {
        public InputActionReference ActionRef;
        public Button RebindButton;
        public TMP_Text BindingLabel;

        [NonSerialized] public InputAction Action;
    }

    [Header("Sensitivity / Invert")]
    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private TMP_Text _sensitivityValueLabel;
    [SerializeField] private Toggle _invertToggle;

    [Header("Rebinding")]
    [SerializeField] private List<RebindEntry> _rebindEntries;

    private InputActionRebindingExtensions.RebindingOperation _activeRebind;

    private void OnEnable()
    {
        SetupSensitivity();
        SetupInvert();
        SetupRebindEntries();
    }

    private void OnDisable()
    {
        _activeRebind?.Dispose();
    }

    private void SetupSensitivity()
    {
        var current = GameSettingsService.Instance.Current;

        _sensitivitySlider.SetValueWithoutNotify(current.MouseSensitivity);
        _sensitivityValueLabel.text = current.MouseSensitivity.ToString(SettingsConstants.Format.SensitivityValue);

        _sensitivitySlider.onValueChanged.RemoveAllListeners();
        _sensitivitySlider.onValueChanged.AddListener(v =>
        {
            _sensitivityValueLabel.text = v.ToString(SettingsConstants.Format.SensitivityValue);
            GameSettingsService.Instance.SetMouseSensitivity(v);
        });
    }

    private void SetupInvert()
    {
        var current = GameSettingsService.Instance.Current;

        _invertToggle.SetIsOnWithoutNotify(current.InvertCamera);
        _invertToggle.onValueChanged.RemoveAllListeners();
        _invertToggle.onValueChanged.AddListener(v => GameSettingsService.Instance.SetInvertCamera(v));
    }

    private void SetupRebindEntries()
    {
        foreach (var entry in _rebindEntries)
        {
            entry.Action = InputRebindService.Instance.GetAction(entry.ActionRef.action.name);
            RefreshLabel(entry);

            entry.RebindButton.onClick.RemoveAllListeners();
            entry.RebindButton.onClick.AddListener(() => StartRebind(entry));
        }
    }

    private void StartRebind(RebindEntry entry)
    {
        entry.Action.Disable();
        entry.BindingLabel.text = SettingsConstants.UiText.RebindPrompt;

        _activeRebind = entry.Action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse/position")
            .WithControlsExcluding("Mouse/delta")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                op.Dispose();
                entry.Action.Enable();
                RefreshLabel(entry);
                InputRebindService.Instance.SaveCurrentBindings();
            })
            .OnCancel(op =>
            {
                op.Dispose();
                entry.Action.Enable();
                RefreshLabel(entry);
            })
            .Start();
    }

    private void RefreshLabel(RebindEntry entry)
    {
        entry.BindingLabel.text = InputControlPath.ToHumanReadableString(
            entry.Action.bindings[0].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}