using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using UnityEngine;

public class TransportSwitcher : MonoBehaviour
{
    public void UseTugboat()
    {
        GetComponent<Multipass>().SetClientTransport<Tugboat>();
        PlayerPrefs.SetInt("UseLocalTransport", 1);
        Debug.Log("Transport: Tugboat");
    }

    public void UseFishyFacepunch()
    {
        GetComponent<Multipass>().SetClientTransport<FishyFacepunch.FishyFacepunch>();
        PlayerPrefs.SetInt("UseLocalTransport", 0);
        Debug.Log("Transport: FishyFacepunch");
    }
}