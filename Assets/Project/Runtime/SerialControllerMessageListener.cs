using Newtonsoft.Json.Serialization;
using UnityEngine;

public class SerialControllerMessageListener : MonoBehaviour
{
    [SerializeField] private ConnectDevice cd;

    private bool _isSuccessfullyConnected = false;
    private bool _receivedValidGreeting = false;

    // Invoked when a line of data is received from the serial device.
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void OnMessageArrived(string msg)
    {
        if (!_receivedValidGreeting && _isSuccessfullyConnected)
        {
            _receivedValidGreeting = msg is "Re:[] new connection" or "Re:[] re-connection";
            SetConnectionStatus(_receivedValidGreeting);
        }

        Debug.Log("Inbound response: " + msg);
    }

    // Invoked when a connect/disconnect event occurs. The parameter 'success'
    // will be 'true' upon connection, and 'false' upon disconnection or
    // failure to connect.
    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void OnConnectionEvent(bool success)
    {
        if (!success) SetConnectionStatus(false);
        else _isSuccessfullyConnected = true;
    }

    private void SetConnectionStatus(bool success)
    {
        // Manage ConnectDevice state
        if (success) cd.ConnectDevicePanel.SetActive(false);
        else cd.ConnectingText.alpha = 0;

        // Log connection status
        Debug.Log(success ? "Connection established" : "Connection attempt failed or disconnection detected");
    }
}
