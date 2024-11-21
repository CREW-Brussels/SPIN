using System;
using OscJack;
using UnityEngine;
using Wave.Essence.Tracker;
using Wave.Native;

namespace Brussels.Crew.Spin.Spin
{

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

                if (SpinConfigManager.OSCTrackersConfig.TrackerIds.Length <= (int)trackerId)
                    Array.Resize(ref SpinConfigManager.OSCTrackersConfig.TrackerIds, (int)trackerId + 1);


                if (SpinConfigManager.OSCTrackersConfig.TrackerIds[(int)trackerId] == null || string.IsNullOrEmpty(SpinConfigManager.OSCTrackersConfig.TrackerIds[(int)trackerId].Name))
                {
                    SpinConfigManager.OSCTrackersConfig.TrackerIds[(int)trackerId] = new TrackerConfig();
                }

                GameObject TrackerInstance = Instantiate(TrackerPrefab, transform);

                TrackerInstance.transform.name = trackerId.ToString();
                SpinConfigManager.OSCTrackersConfig.TrackerIds[(int)trackerId].tracker = TrackerInstance.GetComponent<Tracker>();
                SpinConfigManager.OSCTrackersConfig.TrackerIds[(int)trackerId].tracker.Init(trackerId);
            }

            foreach (WVR_DeviceType deviceType in new WVR_DeviceType[] { WVR_DeviceType.WVR_DeviceType_HMD, WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_DeviceType.WVR_DeviceType_Controller_Right })
            {
                if (deviceType == WVR_DeviceType.WVR_DeviceType_Invalid) continue;
                int id = SpinConfigManager.OSCTrackersConfig.TrackerIds.Length;
                Array.Resize(ref SpinConfigManager.OSCTrackersConfig.TrackerIds, id + 1);
                
                if (SpinConfigManager.OSCTrackersConfig.TrackerIds[id] == null || string.IsNullOrEmpty(SpinConfigManager.OSCTrackersConfig.TrackerIds[id].Name))
                {
                    SpinConfigManager.OSCTrackersConfig.TrackerIds[id] = new TrackerConfig();
                }
                
                GameObject TrackerInstance = Instantiate(TrackerPrefab, transform);

                TrackerInstance.transform.name = deviceType.ToString();
                SpinConfigManager.OSCTrackersConfig.TrackerIds[id].tracker = TrackerInstance.GetComponent<Tracker>();
                SpinConfigManager.OSCTrackersConfig.TrackerIds[id].tracker.Init(deviceType, id);
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

            foreach (SpinConnection connection in SpinConfigManager.OSCTrackersConfig.Servers)
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

}