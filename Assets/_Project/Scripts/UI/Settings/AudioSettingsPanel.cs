using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [SerializeField] private TMP_Text _masterValueLabel;
    [SerializeField] private TMP_Text _musicValueLabel;
    [SerializeField] private TMP_Text _sfxValueLabel;

    private void OnEnable()
    {
        var current = GameSettingsService.Instance.Current;

        SetupSlider(_masterSlider, _masterValueLabel, current.MasterVolume, VolumeChannel.Master);
        SetupSlider(_musicSlider, _musicValueLabel, current.MusicVolume, VolumeChannel.Music);
        SetupSlider(_sfxSlider, _sfxValueLabel, current.SfxVolume, VolumeChannel.Sfx);
    }

    private void SetupSlider(Slider slider, TMP_Text label, float value, VolumeChannel channel)
    {
        slider.SetValueWithoutNotify(value);
        label.text = value.ToString(SettingsConstants.Format.VolumeValue);

        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(v =>
        {
            label.text = v.ToString(SettingsConstants.Format.VolumeValue);
            GameSettingsService.Instance.SetVolume(channel, v);
        });
    }
}