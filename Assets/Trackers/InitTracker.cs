using OscJack;
using System;

using UnityEngine;
using Wave.Essence.Tracker;

public class InitTracker : MonoBehaviour
{
    public OscConnection oscConnection;
    public string oscAddress;

    public OscClient oscClient;

    public GameObject TrackerPrefab;

    void Start()
    {
        TrackerManager.Instance.StartTracker();

        if (string.IsNullOrEmpty(oscAddress))
        {
            oscAddress = SystemInfo.deviceName;
            if (string.IsNullOrEmpty(oscAddress) || oscAddress == "<unknown>")
                oscAddress = "Spin";
        }

        foreach (TrackerId trackerId in (TrackerId[])Enum.GetValues(typeof(TrackerId)))
        {
            GameObject TrackerInstance = Instantiate(TrackerPrefab, transform);
            TrackerInstance.transform.name = trackerId.ToString();
            TrackerInstance.GetComponent<Tracker>().Init(trackerId, this);
        }

        ConnectOSC();
    }

    public void ConnectOSC()
    {
        if (oscClient != null)
        {
            oscClient.Dispose();
            oscClient = null;
        }

        oscClient = new OscClient(oscConnection.host, oscConnection.port);
    }
}
