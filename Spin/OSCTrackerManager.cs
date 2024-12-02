using System;
using OscJack;
using UnityEngine;
using Wave.Essence.Tracker;
using Wave.Native;

namespace Brussels.Crew.Spin.Spin
{
    /// <summary>
    /// Manages the instantiation and handling of tracker objects in the Unity environment.
    /// </summary>
    /// <remarks>
    /// The OSCTrackerManager class is designed to facilitate the management of tracker entities
    /// within a Unity scene. It allows for the dynamic creation and handling of such trackers,
    /// typically used for motion tracking or similar functionalities.
    /// </remarks>
    public class OSCTrackerManager : MonoBehaviour
    {
        /// <summary>
        /// A public GameObject reference used for instantiating tracker instances within the
        /// OSCTrackerManager. This prefab serves as the blueprint for creating trackers that
        /// will be managed and configured according to the application’s OSC tracker settings.
        /// It is essential for facilitating the communication and functionality of virtual
        /// tracker objects within the Unity environment.
        /// </summary>
        public GameObject TrackerPrefab;

        /// <summary>
        /// The SpinConfigManager class is responsible for managing and maintaining the configuration related to Spin trackers in the application.
        /// </summary>
        /// <remarks>
        /// This singleton class provides functionality to interact with OSC trackers configuration,
        /// manage the connection and default settings, as well as save and clear Spin-related configuration data.
        /// </remarks>
        private SpinConfigManager SpinConfigManager;

        /// <summary>
        /// Initializes the tracker manager and orchestrates the setup process for tracker instances.
        /// This method performs several initialization steps:
        /// 1. Starts the tracker through the `TrackerManager` instance.
        /// 2. Subscribes to the configuration update event from `SpinConfigManager` to handle updates dynamically.
        /// 3. Invokes the `SpawnTrackerInstances` method to create and configure tracker instances based on available tracker ids and device types.
        /// 4. Saves the current spin configuration.
        /// </summary>
        void Start()
        {
            TrackerManager.Instance.StartTracker();

            SpinConfigManager = SpinConfigManager.Instance;

            SpinConfigManager.ConfigUpdatedEvent += ConfigUpdated;

            SpawnTrackerInstances();

            SpinConfigManager.SaveSpinConfig();
        }

        /// This method is triggered when the configuration is updated. It is subscribed to the ConfigUpdatedEvent
        /// of the SpinConfigManager. When invoked, it establishes connections to OSC servers by calling the
        /// ConnectToOscServers method, ensuring that the OSC tracker system operates with the most current
        /// configuration settings.
        private void ConfigUpdated()
        {
            ConnectToOscServers();
        }

        /// Initializes and spawns tracker instances based on the current configuration.
        /// This method goes through all possible tracker IDs, creates new tracker instances
        /// if necessary, and initializes them with relevant configuration settings. It handles
        /// both predefined tracker IDs and device types like HMD and controllers.
        /// The method also ensures that the tracker IDs array in the configuration is properly
        /// sized and that each tracker instance is correctly initialized and associated with its
        /// corresponding configuration entry. Finally, it saves the updated configuration.
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

        /// Disconnects from all OSC (Open Sound Control) servers currently connected through the
        /// OSCTrackersConfig's oscClients list. This method iterates over all entries in the
        /// oscClients list and disposes each OscClient, ensuring that network resources are properly
        /// released. After disconnecting, it clears the oscClients list to ensure it accurately
        /// reflects the current state, which is that no clients remain connected.
        private void DisconnectFromOscServers()
        {
            foreach (OscClient client in SpinConfigManager.OSCTrackersConfig.oscClients)
            {
                if (client != null)
                    client.Dispose();
            }
            SpinConfigManager.OSCTrackersConfig.oscClients.Clear();
        }

        /// <summary>
        /// Establishes connections to multiple OSC servers defined in the configuration.
        /// </summary>
        /// <remarks>
        /// This method first disconnects from any existing OSC server connections and then iterates through
        /// the configured servers to establish new connections. Each successful connection creates an
        /// OscClient instance which is stored in the configuration for later use.
        /// </remarks>
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

        /// Called when the OSCTrackerManager is destroyed.
        /// This method disconnects from all OSC servers and unregisters the ConfigUpdated event listener.
        /// It ensures that resources are properly released and event subscriptions are cleaned up to
        /// prevent memory leaks or unexpected behaviors.
        private void OnDestroy()
        {
            DisconnectFromOscServers();
            SpinConfigManager.ConfigUpdatedEvent -= ConfigUpdated;

        }
    }

}