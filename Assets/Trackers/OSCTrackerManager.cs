using OscJack;
using System;

using UnityEngine;
using Wave.Essence.Tracker;

public class OSCTrackerManager : MonoBehaviour
{
    public OscConnection oscConnection;

    public string OSCDeviceName;

    public OscClient oscClient;

    public GameObject TrackerPrefab;

    void Start()
    {
        TrackerManager.Instance.StartTracker();

        if (string.IsNullOrEmpty(OSCDeviceName))
        {
            OSCDeviceName = SystemInfo.deviceName;
            if (string.IsNullOrEmpty(OSCDeviceName) || OSCDeviceName == "<unknown>")
                OSCDeviceName = "Spin";
        }

        foreach (TrackerId trackerId in (TrackerId[])Enum.GetValues(typeof(TrackerId)))
        {
            GameObject TrackerInstance = Instantiate(TrackerPrefab, transform);
            TrackerInstance.transform.name = trackerId.ToString();
            TrackerInstance.GetComponent<Tracker>().Init(trackerId, this);
        }

        ConnectOSCClients();
    }

    private void OnDestroy()
    {
        DisconnectOSCClients();
    }

    private void DisconnectOSCClients()
    {
        if (oscClient != null)
        {
            oscClient.Dispose();
            oscClient = null;
        }
    }

    public void ConnectOSCClients()
    {
        DisconnectOSCClients();

        oscClient = new OscClient(oscConnection.host, oscConnection.port);
    }
}
