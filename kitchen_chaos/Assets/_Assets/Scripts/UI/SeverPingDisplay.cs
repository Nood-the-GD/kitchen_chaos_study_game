using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class SeverPingDisplay : MonoBehaviourPunCallbacks
{
    public Text statusText;
    private float pingUpdateInterval = 2.0f;
    private float nextPingUpdateTime;

    private void Start()
    {
        statusText = GetComponent<Text>();
        statusText.text = "Connecting...";
        nextPingUpdateTime = Time.time + pingUpdateInterval;
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Update the ping every 3 seconds
            if (Time.time >= nextPingUpdateTime)
            {
                UpdatePingAndServerInfo();
                nextPingUpdateTime = Time.time + pingUpdateInterval;
            }
        }
        else
        {
            statusText.text = "Connecting...";
        }
    }

    private void UpdatePingAndServerInfo()
    {
        var pings = PhotonManager.allRegionPing;
        if (pings.Count == 0)
        {
            statusText.text = "Connecting...";
            return;
        }

        // Get the current ping
        int ping = pings.Find(x => x.region == UserSetting.regionSelected).ping;
        // Get the current server name
        string serverRegion = UserSetting.regionSelected;

        // Display the server name and ping in the UI Text
        statusText.text = "Server: " + serverRegion + " " + ping + " ms";
        statusText.color = PingHightight(ping);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon server");

        // Update the server name and ping info once connected
        UpdatePingAndServerInfo();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Disconnected from Photon server: " + cause);
        statusText.text = "Disconnected";
    }


    Color PingHightight(int ping)
    {
        if (ping < 100)
            return Color.green;
        else if (ping < 170)
            return Color.yellow;
        else
            return Color.red;
    }


}
