using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using Wave.Essence.Tracker;

public class Tracker : MonoBehaviour
{
    public GameObject Canvas;
    public TMP_Text Name;
    public TMP_Text ID;
    public TMP_Text Address;
    public TMP_Text Status;
    public TMP_Text Bat;

#if UNITY_EDITOR
    public bool Debug = false;
#endif

    private string OSCAddress;
    private TrackerManager trackerManager;
    private TrackerId TrackerId;
    private SpinConfigManager spinConfigManager;

    public void Init(TrackerId trackerId, OSCTrackerConfig TrackersConfig)
    {
        spinConfigManager = SpinConfigManager.Instance;
        spinConfigManager.ConfigUpdatedEvent += ConfigUpdateEvent;
        trackerManager = TrackerManager.Instance;
        TrackerId = trackerId;

        if (ID != null )
            ID.text = trackerId.ToString();
        if (Name != null )
            Name.text = trackerManager.GetTrackerDeviceName(trackerId);
        spinConfigManager.OSCTrackersConfig.TrackerIds[trackerId].Name = trackerManager.GetTrackerDeviceName(trackerId);

        UpdateOSCAddress();
        SpinConfigManager.Instance.SaveSpinConfig();
    }

    private void ConfigUpdateEvent() => UpdateOSCAddress();

    public string UpdateOSCAddress(string adr = null)
    {
        if (string.IsNullOrEmpty(spinConfigManager.OSCTrackersConfig.TrackersRoles[spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].Role]))
            spinConfigManager.OSCTrackersConfig.TrackersRoles[spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].Role] = TrackerId.ToString();

        if (adr == null)
            OSCAddress = "/" + spinConfigManager.OSCTrackersConfig.OSCDeviceName.Trim('/').Trim() + "/" + spinConfigManager.OSCTrackersConfig.TrackersRoles[spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].Role].Trim('/').Trim();
        else
            OSCAddress = adr;

        if (Address != null)
            Address.text = OSCAddress;

        return OSCAddress;
    }

    private bool ShowInfo
    {
        get
        {
            return _ShowInfo;
        }
        set
        {
            if (_ShowInfo != value)
            {
                _ShowInfo = value;
                if (Canvas != null)
                    Canvas.SetActive(value);
                spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].Online = value;
            }
        }
    }
    private bool _ShowInfo = true;

    private float BatteryValue
    {
        get
        {
            return _BatteryValue;
        }
        set
        {
            if (_BatteryValue != value)
            {
                _BatteryValue = value;
                if (Bat != null)
                    Bat.text = "Battery " + trackerManager.GetTrackerBatteryLife(TrackerId).ToString("P1", CultureInfo.InvariantCulture);
                spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].Battery = value;
            }
        }
    }
    private float _BatteryValue;

    private InputTrackingState TrackingState
    {
        get
        {
            return _TrackingState;
        }
        set
        {
            if (_TrackingState != value)
            {
                _TrackingState = value;
                if (Status != null)
                {
                    string pos = (value & InputTrackingState.Position) != 0 ? "green" : "red";
                    string rot = (value & InputTrackingState.Rotation) != 0 ? "green" : "red";

                    Status.richText = true;
                    Status.text = $"<color=\"{pos}\">Position <color=\"{rot}\">Rotation";
                }
                spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].TrackingPosition = (value & InputTrackingState.Position) != 0;
                spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].TrackingRotation = (value & InputTrackingState.Rotation) != 0;
            }
        }
    }
    private InputTrackingState _TrackingState;
    private void SendOSCMessage()
    {
#if UNITY_EDITOR
        if (spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].Active || Debug)
#else
        if (spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].Active)
#endif
        {
            foreach (int server in spinConfigManager.OSCTrackersConfig.TrackerIds[TrackerId].Servers)
            {
                if (spinConfigManager.OSCTrackersConfig.oscClients.Count >= server && spinConfigManager.OSCTrackersConfig.oscClients[server] != null)
                {
                    spinConfigManager.OSCTrackersConfig.oscClients[server].Send(OSCAddress + "/Position", transform.position.x, transform.position.y, transform.position.z);
                    spinConfigManager.OSCTrackersConfig.oscClients[server].Send(OSCAddress + "/Rotation", transform.rotation.w, transform.rotation.x, transform.rotation.y, transform.rotation.z);
                    spinConfigManager.OSCTrackersConfig.oscClients[server].Send(OSCAddress + "/Battery", trackerManager.GetTrackerBatteryLife(TrackerId));
                }
            }
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (trackerManager.IsTrackerConnected(TrackerId) || Debug)
#else
        if (trackerManager.IsTrackerConnected(TrackerId))
#endif
        {
            ShowInfo = true;

#if UNITY_EDITOR
            transform.position = !Debug ? trackerManager.GetTrackerPosition(TrackerId) : GetFakePosition();
            transform.rotation = !Debug ? trackerManager.GetTrackerRotation(TrackerId) : GetFakeRotation();
#else
            transform.position = trackerManager.GetTrackerPosition(TrackerId);
            transform.rotation = trackerManager.GetTrackerRotation(TrackerId);
#endif
            BatteryValue = trackerManager.GetTrackerBatteryLife(TrackerId);
            InputTrackingState TS;
            trackerManager.GetTrackerTrackingState(TrackerId, out TS);

            TrackingState = TS;

            SendOSCMessage();
        }
        else
            ShowInfo = false;
    }

#if UNITY_EDITOR
    private Quaternion GetFakeRotation()
    {
        return Quaternion.Euler(.1f * Time.frameCount % 360, 0f, 0f);
    }

    private Vector3 GetFakePosition()
    {
        return new Vector3(Mathf.Cos(.1f * Time.frameCount), Mathf.Sin(.1f * Time.frameCount));
    }
#endif
    private void OnDestroy()
    {
        spinConfigManager.ConfigUpdatedEvent -= ConfigUpdateEvent;
    }

}