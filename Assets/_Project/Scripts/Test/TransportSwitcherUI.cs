using FishNet;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransportSwitcherUI : MonoBehaviour
{
    [SerializeField] private Button _switchButton;
    [SerializeField] private TMP_Text _currentTransportText;

    private Multipass _multipass;
    private bool _isFishyFacepunch = true;

    private void Awake()
    {
        _switchButton.onClick.AddListener(ToggleTransport);
    }

    private void Start()
    {
        _multipass = InstanceFinder.NetworkManager.GetComponent<Multipass>();

        if (_multipass == null)
        {
            Debug.LogError("[TransportSwitcherUI] Multipass not found on NetworkManager!");
            return;
        }

        UpdateText();
    }

    private void ToggleTransport()
    {
        if (_multipass == null)
            return;

        if (_isFishyFacepunch)
            UseTugboat();
        else
            UseFishyFacepunch();
    }

    private void UseTugboat()
    {
        _multipass.SetClientTransport<Tugboat>();
        PlayerPrefs.SetInt("UseLocalTransport", 1);
        _isFishyFacepunch = false;
        UpdateText();
        Debug.Log("Transport: Tugboat");
    }

    private void UseFishyFacepunch()
    {
        _multipass.SetClientTransport<FishyFacepunch.FishyFacepunch>();
        PlayerPrefs.SetInt("UseLocalTransport", 0);
        _isFishyFacepunch = true;
        UpdateText();
        Debug.Log("Transport: FishyFacepunch");
    }

    private void UpdateText()
    {
        _currentTransportText.text = $"current: {(_isFishyFacepunch ? "FishyFacepunch" : "Tugboat")}";
    }
}