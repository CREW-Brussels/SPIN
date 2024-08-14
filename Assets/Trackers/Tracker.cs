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

    public string TrackerRole;

    private TrackerManager trackerManager;
    private TrackerId TrackerId;
    private string OSCAddress;
    private InitTracker InitTracker;

    public void Init(TrackerId trackerId, InitTracker initTracker)
    {
        this.InitTracker = initTracker;  
        trackerManager = TrackerManager.Instance;
        TrackerId = trackerId;

        if (ID != null )
            ID.text = trackerId.ToString();
        if (Name != null )
            Name.text = trackerManager.GetTrackerDeviceName(trackerId);

        UpdateOSCAddress();
    }

    public string UpdateOSCAddress(string adr = null)
    {
        if (string.IsNullOrEmpty(TrackerRole))
            TrackerRole = TrackerId.ToString();

        if (adr == null)
            OSCAddress = "/" + InitTracker.oscAddress.Trim('/').Trim() + "/" + TrackerRole.Trim('/').Trim();
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
            }
        }
    }
    private bool _ShowInfo;

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
            }
        }
    }
    private InputTrackingState _TrackingState;
    private void SendOSCMessage()
    {
        InitTracker.oscClient.Send(OSCAddress + "/Position", transform.position.x, transform.position.y, transform.position.z);
        InitTracker.oscClient.Send(OSCAddress + "/Rotation", transform.rotation.w, transform.rotation.x, transform.rotation.y, transform.rotation.z);
        InitTracker.oscClient.Send(OSCAddress + "/Battery", trackerManager.GetTrackerBatteryLife(TrackerId));
    }

    void Update()
    {
        if (trackerManager.IsTrackerConnected(TrackerId))
        {
            ShowInfo = true;

            transform.position = trackerManager.GetTrackerPosition(TrackerId);
            transform.rotation = trackerManager.GetTrackerRotation(TrackerId);

            BatteryValue = trackerManager.GetTrackerBatteryLife(TrackerId);
            InputTrackingState TS;
            trackerManager.GetTrackerTrackingState(TrackerId, out TS);

            TrackingState = TS;

            SendOSCMessage();
        }
        else
            ShowInfo = false;
    }

}
