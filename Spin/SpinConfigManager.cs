using UnityEngine;

namespace Brussels.Crew.Spin.Spin
{
    /// <summary>
    /// Manages configuration settings related to Spin trackers within the application.
    /// </summary>
    /// <remarks>
    /// This class serves as a singleton, ensuring centralized control over SpinTracker configurations,
    /// managing defaults and connections, and handling functionalities to save and clear configuration data.
    /// </remarks>
    public class SpinConfigManager : MonoBehaviour
    {
        /// <summary>
        /// Represents the default SpinConnection instance used for configuration purposes
        /// within the SpinConfigManager class. It is used to initialize the list of servers
        /// in OSCTrackersConfig when no servers are currently configured.
        /// </summary>
        public SpinConnection DefaultServer;

        /// <summary>
        /// The default name assigned to a device for OSC (Open Sound Control) connections
        /// within the Spin configuration system. This value is used when a specific device
        /// name is not provided or when the system cannot determine the device name.
        /// The default setting helps ensure that OSC connections can be established effectively
        /// with a predefined identifier.
        /// </summary>
        public string DefaultDeviceName = "Spin";

        /// <summary>
        /// OSCTrackersConfig is a variable of type OSCTrackerConfig. It holds the configuration data for OSC (Open Sound Control) trackers.
        /// This includes device name, refresh rate, a list of server connections, tracker roles, and an array of tracker IDs.
        /// It also manages a list of OSC clients used for network communication within the application.
        /// </summary>
        public OSCTrackerConfig OSCTrackersConfig = new OSCTrackerConfig();

        /// <summary>
        /// Delegate type representing a method that handles configuration updates.
        /// Intended to be used for events or methods that are triggered when the configuration
        /// of the Spin system changes.
        /// </summary>
        public delegate void ConfigUpdated();

        /// <summary>
        /// Represents an event that is invoked when the spin configuration is updated.
        /// </summary>
        /// <remarks>
        /// This event is utilized to notify subscribers whenever the configuration settings
        /// managed by the <c>SpinConfigManager</c> are modified and saved. Listeners can
        /// subscribe to this event to handle any custom logic that needs to be executed
        /// after a configuration update.
        /// The event is fired in the <c>Update</c> method of <c>SpinConfigManager</c> whenever
        /// the configuration changes are saved successfully.
        /// </remarks>
        public ConfigUpdated ConfigUpdatedEvent;

        /// <summary>
        /// Represents a boolean flag used to determine whether to send a message or command during the update loop.
        /// This flag is set to true when certain conditions, such as the refresh rate and elapsed time, are met.
        /// </summary>
        public bool Send;

        /// <summary>
        /// Singleton instance of the SpinConfigManager, responsible for managing the spin configuration settings.
        /// </summary>
        private static SpinConfigManager instance = null;

        /// <summary>
        /// Gets the singleton instance of the <see cref="SpinConfigManager"/> class.
        /// This property provides access to the single, shared instance of the
        /// <see cref="SpinConfigManager"/> throughout the application, ensuring that all
        /// components have consistent and synchronized access to the configuration manager.
        /// </summary>
        public static SpinConfigManager Instance => instance;

        /// <summary>
        /// Initializes the singleton instance of the SpinConfigManager.
        /// </summary>
        /// <remarks>
        /// The Awake method is typically used for any initialization logic
        /// necessary when a Unity component is first run. In the SpinConfigManager,
        /// the Awake method sets the static instance to the current instance of
        /// the SpinConfigManager, allowing it to function as a singleton.
        /// </remarks>
        private void Awake()
        {
            instance = this;
            //ClearSpinConfig();
        }

        /// <summary>
        /// Initializes the SpinConfigManager by restoring the configuration settings.
        /// This method is automatically called at the start of the component's lifecycle.
        /// It attempts to load the configuration from the stored player preferences
        /// and updates the OSCTrackerConfig and related settings. If necessary, it falls back on
        /// default configurations and ensures that at least one server is present in the configuration.
        /// </summary>
        void Start() => RestoreSpinConfig();

        /// <summary>
        /// Restores the Spin configuration by attempting to load saved configuration
        /// from player preferences. If no saved configuration exists, default values
        /// are used. This includes the addition of a default server and setting the
        /// device name. The restored or default configuration is then saved.
        /// </summary>
        private void RestoreSpinConfig()
        {
            if (PlayerPrefs.HasKey("SpinConfig" + Application.version))
            {
                Debug.Log("Config load " + PlayerPrefs.GetString("SpinConfig" + Application.version));
                OSCTrackersConfig = JsonUtility.FromJson<OSCTrackerConfig>(PlayerPrefs.GetString("SpinConfig" + Application.version));
            }

            if (OSCTrackersConfig.Servers.Count == 0)
            {
                OSCTrackersConfig.Servers.Add(DefaultServer);
                SaveSpinConfig();
            }

            if (string.IsNullOrEmpty(OSCTrackersConfig.OSCDeviceName))
            {
                OSCTrackersConfig.OSCDeviceName = SystemInfo.deviceName;
                if (string.IsNullOrEmpty(OSCTrackersConfig.OSCDeviceName) || OSCTrackersConfig.OSCDeviceName == "<unknown>")
                    OSCTrackersConfig.OSCDeviceName = DefaultDeviceName;
                SaveSpinConfig();
            }
        }

        /// <summary>
        /// Represents a flag that indicates whether the configuration needs to be saved.
        /// When set to true, the current configuration data is serialized and stored.
        /// </summary>
        private bool Save;

        /// <summary>
        /// Saves the current Spin configuration settings.
        /// This method sets the internal flag to save, indicating that the current configuration
        /// should be preserved. It is utilized internally when specific configuration updates occur,
        /// such as changes to the server list or device name.
        /// </summary>
        public void SaveSpinConfig()
        {
            Save = true;
        }

        /// <summary>
        /// Represents the timestamp of the last message sent.
        /// This variable is used to manage the rate at which messages are sent
        /// based on the configured OSC refresh rate. It is updated each time
        /// a message is sent to ensure that the interval between messages
        /// adheres to the specified refresh rate.
        /// </summary>
        private float LastMessage;

        /// <summary>
        /// Updates the state of the SpinConfigManager at each frame. This method is responsible
        /// for determining the need to send OSC messages based on the refresh rate and elapsed time.
        /// It also handles the saving of the configuration when the save flag is set and triggers
        /// the ConfigUpdatedEvent if any configuration changes occur.
        /// </summary>
        private void Update()
        {
            Send = false;

            if (OSCTrackersConfig.OSCRefreshRate < 1)
                OSCTrackersConfig.OSCRefreshRate = 60;
            
            if (Time.time - LastMessage >= 1 / (float)OSCTrackersConfig.OSCRefreshRate)
            {
                Send = true;
                LastMessage = Time.time;
            }

            if (Save)
            {
                Save = false;
                string config = JsonUtility.ToJson(OSCTrackersConfig);
                PlayerPrefs.SetString("SpinConfig" + Application.version, config);
                if (ConfigUpdatedEvent != null)
                    ConfigUpdatedEvent.Invoke();
            }
        }

        /// <summary>
        /// Clears all stored player preferences related to the spin configuration
        /// and restores the spin configuration to its default state.
        /// </summary>
        public void ClearSpinConfig()
        {
            PlayerPrefs.DeleteAll();
            RestoreSpinConfig();
        }
    }

}