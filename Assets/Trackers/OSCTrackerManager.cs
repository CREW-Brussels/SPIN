using OscJack;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Wave.Essence.Tracker;

[Serializable]
public class OSCTrackerConfig
{
    [Serializable]
    public class TrackerConfig
    {
        public int[] Servers;
        public int Role;
        public float Battery;
        public bool Active;
        public bool Online;
        public string Name;
        public bool TrackingPosition;
        public bool TrackingRotation;
        public Tracker tracker;
    }

    public string OSCDeviceName;

    public List<OscConnection> Servers = new List<OscConnection>();
    public List<string> TrackersRoles = new List<string>();
    public List<OscClient> oscClients = new List<OscClient>();
    public Dictionary<TrackerId, TrackerConfig> TrackerIds = new Dictionary<TrackerId, TrackerConfig>();
}

public class OSCTrackerManager : MonoBehaviour
{
    public GameObject TrackerPrefab;

    //TODO: make persistent
    public OSCTrackerConfig OSCTrackersConfig;

    void Start()
    {
        TrackerManager.Instance.StartTracker();

        SetDeviceName();

        SpawnTrackerInstances();

        ConnectToOscServers();
    }

    private void SpawnTrackerInstances()
    {
        foreach (TrackerId trackerId in (TrackerId[])Enum.GetValues(typeof(TrackerId)))
        {
            if (!OSCTrackersConfig.TrackerIds.ContainsKey(trackerId))
                OSCTrackersConfig.TrackerIds.Add(trackerId, new OSCTrackerConfig.TrackerConfig());


            GameObject TrackerInstance = Instantiate(TrackerPrefab, transform);

            TrackerInstance.transform.name = trackerId.ToString();
            OSCTrackersConfig.TrackerIds[trackerId].tracker = TrackerInstance.GetComponent<Tracker>();
            OSCTrackersConfig.TrackerIds[trackerId].tracker.Init(trackerId, OSCTrackersConfig);
        }
    }

    private void DisconnectFromOscServers()
    {
        foreach (OscClient client in OSCTrackersConfig.oscClients)
        {
            if (client != null)
                client.Dispose();
            OSCTrackersConfig.oscClients.Clear();
        }
    }

    private void ConnectToOscServers()
    {
        DisconnectFromOscServers();

        foreach (OscConnection connection in OSCTrackersConfig.Servers)
        {
            OscClient client = new OscClient(connection.host, connection.port);
            OSCTrackersConfig.oscClients.Add(client);
        }
    }

    private void SetDeviceName()
    {
        if (string.IsNullOrEmpty(OSCTrackersConfig.OSCDeviceName))
        {
            OSCTrackersConfig.OSCDeviceName = SystemInfo.deviceName;
            if (string.IsNullOrEmpty(OSCTrackersConfig.OSCDeviceName) || OSCTrackersConfig.OSCDeviceName == "<unknown>")
                OSCTrackersConfig.OSCDeviceName = "Spin";
        }
    }

    private void OnDestroy()
    {
        DisconnectFromOscServers();
    }
}
