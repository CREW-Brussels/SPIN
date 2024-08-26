using UnityEngine;

namespace Brussels.Crew.Spin
{

    public class SpinConfigManager : MonoBehaviour
    {
        public SpinConnection DefaultServer;

        public string DefaultDeviceName = "Spin";

        public OSCTrackerConfig OSCTrackersConfig = new OSCTrackerConfig();

        public delegate void ConfigUpdated();
        public ConfigUpdated ConfigUpdatedEvent;

        private static SpinConfigManager instance = null;
        public static SpinConfigManager Instance => instance;

        private void Awake()
        {
            instance = this;
            //ClearSpinConfig();
        }

        void Start() => RestoreSpinConfig();

        private void RestoreSpinConfig()
        {
            if (PlayerPrefs.HasKey("SpinConfig"))
            {
                Debug.Log("Config load " + PlayerPrefs.GetString("SpinConfig"));
                OSCTrackersConfig = JsonUtility.FromJson<OSCTrackerConfig>(PlayerPrefs.GetString("SpinConfig"));
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

        private bool Save;

        public void SaveSpinConfig()
        {
            Save = true;
        }

        private void FixedUpdate()
        {
            if (Save)
            {
                Save = false;
                string config = JsonUtility.ToJson(OSCTrackersConfig);
                Debug.Log("Config save " + config);
                PlayerPrefs.SetString("SpinConfig", config);
                if (ConfigUpdatedEvent != null)
                    ConfigUpdatedEvent.Invoke();
            }
        }

        public void ClearSpinConfig()
        {
            PlayerPrefs.DeleteAll();
            RestoreSpinConfig();
        }
    }

}