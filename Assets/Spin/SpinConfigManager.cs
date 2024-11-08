using UnityEngine;

namespace Brussels.Crew.Spin.Spin
{

    public class SpinConfigManager : MonoBehaviour
    {
        public SpinConnection DefaultServer;

        public string DefaultDeviceName = "Spin";

        public OSCTrackerConfig OSCTrackersConfig = new OSCTrackerConfig();

        public delegate void ConfigUpdated();
        public ConfigUpdated ConfigUpdatedEvent;

        public bool Send;

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

        private bool Save;

        public void SaveSpinConfig()
        {
            Save = true;
        }

        private float LastMessage;

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
//                Debug.Log("Config save " + config);
                PlayerPrefs.SetString("SpinConfig" + Application.version, config);
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