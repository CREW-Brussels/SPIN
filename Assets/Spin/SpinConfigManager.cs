using OscJack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Wave.Essence.Tracker;

[Serializable]
public class Connection
{
    public int port;
    public string host;
    public Connection(int port, string host)
    {
        this.port = port;
        this.host = host;
    }

    OscConnection toOscConnection()
    {
        OscConnection connection = OscConnection.CreateInstance<OscConnection>();
        connection.host = host;
        connection.port = port;
        return connection;
    }
}

[Serializable]
public class TrackerConfig
{
    public List<int> Servers;
    public int Role;
    public float Battery;
    public bool Active;
    public bool Online;
    public string Name;
    public bool TrackingPosition;
    public bool TrackingRotation;
    public Tracker tracker;
}

[Serializable]
public class OSCTrackerConfig
{
    public string OSCDeviceName;

    public List<Connection> Servers = new List<Connection>();
    public List<string> TrackersRoles = new List<string>();
    public List<OscClient> oscClients = new List<OscClient>();
    public Dictionary<TrackerId, TrackerConfig> TrackerIds = new Dictionary<TrackerId, TrackerConfig>();
}


public class SpinConfigManager : MonoBehaviour
{
    public string DefaultServer = "255.255.255.255";
    public int DefaultPort = 8000;
    public string DefaultDeviceName = "Spin";
 
    public OSCTrackerConfig OSCTrackersConfig = new OSCTrackerConfig();

    public delegate void ConfigUpdated();
    public ConfigUpdated ConfigUpdatedEvent;

    private static SpinConfigManager instance = null;
    public static SpinConfigManager Instance => instance;

    private void Awake()
    {
        instance = this;
        ClearSpinConfig();
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
            OSCTrackersConfig.Servers.Add(new Connection(DefaultPort, DefaultServer));
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

    private void LateUpdate()
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
