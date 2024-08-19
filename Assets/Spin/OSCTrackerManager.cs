using OscJack;
using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.Tracker;

public class OSCTrackerManager : MonoBehaviour
{
    public GameObject TrackerPrefab;

    private SpinConfigManager SpinConfigManager;

    void Start()
    {
        TrackerManager.Instance.StartTracker();

        SpinConfigManager = SpinConfigManager.Instance;

        SpinConfigManager.ConfigUpdatedEvent += ConfigUpdated;

        SpawnTrackerInstances();

        SpinConfigManager.SaveSpinConfig();
    }

    private void ConfigUpdated()
    {
        ConnectToOscServers();
    }

    private void SpawnTrackerInstances()
    {
        foreach (TrackerId trackerId in (TrackerId[])Enum.GetValues(typeof(TrackerId)))
        {

            if (!SpinConfigManager.OSCTrackersConfig.TrackerIds.ContainsKey(trackerId))
            {
                SpinConfigManager.OSCTrackersConfig.TrackersRoles.Add(trackerId.ToString());
                SpinConfigManager.OSCTrackersConfig.TrackerIds.Add(trackerId, new TrackerConfig { Active = true, Role = SpinConfigManager.OSCTrackersConfig.TrackersRoles.Count - 1, Servers = new List<int> { 0 } });
            }

            GameObject TrackerInstance = Instantiate(TrackerPrefab, transform);

            TrackerInstance.transform.name = trackerId.ToString();
            SpinConfigManager.OSCTrackersConfig.TrackerIds[trackerId].tracker = TrackerInstance.GetComponent<Tracker>();
            SpinConfigManager.OSCTrackersConfig.TrackerIds[trackerId].tracker.Init(trackerId, SpinConfigManager.OSCTrackersConfig);
        }
        SpinConfigManager.SaveSpinConfig();
    }

    private void DisconnectFromOscServers()
    {
        foreach (OscClient client in SpinConfigManager.OSCTrackersConfig.oscClients)
        {
            if (client != null)
                client.Dispose();
        }
        SpinConfigManager.OSCTrackersConfig.oscClients.Clear();
    }

    private void ConnectToOscServers()
    {
        DisconnectFromOscServers();

        foreach (Connection connection in SpinConfigManager.OSCTrackersConfig.Servers)
        {
            if ( connection != null)
            {
                OscClient client = new OscClient(connection.host, connection.port);
                SpinConfigManager.OSCTrackersConfig.oscClients.Add(client);
            }
        }
    }

    private void OnDestroy()
    {
        DisconnectFromOscServers();
        SpinConfigManager.ConfigUpdatedEvent -= ConfigUpdated;

    }
}
