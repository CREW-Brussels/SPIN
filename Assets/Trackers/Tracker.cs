using OscJack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using Wave.Essence.Tracker;

public class Tracker : MonoBehaviour
{
    public GameObject Canvas;
    public TMP_Text Name;
    public TMP_Text ID;
    public TMP_Text Bat;

    public OscConnection oscConnection;
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
        ID.text = trackerId.ToString();
        Name.text = trackerManager.GetTrackerDeviceName(trackerId);

        if (string.IsNullOrEmpty(TrackerRole))
            TrackerRole = TrackerId.ToString();

        UpdateOSCAddress();
    }

    public string UpdateOSCAddress(string adr = null)
    {
        if (adr == null)
            OSCAddress = "/" + InitTracker.oscAddress.Trim('/').Trim() + "/" + TrackerRole.Trim('/').Trim();
        else
            OSCAddress = adr;

        return OSCAddress;
    }

    void Update()
    {
        if (trackerManager.IsTrackerConnected(TrackerId))
        {
            Canvas.SetActive(true);
            
            transform.position = trackerManager.GetTrackerPosition(TrackerId);
            transform.rotation = trackerManager.GetTrackerRotation(TrackerId);
            Bat.text = "Battery " + trackerManager.GetTrackerBatteryLife(TrackerId).ToString("P1", CultureInfo.InvariantCulture);
            foreach (TrackerButton button in (TrackerButton[])Enum.GetValues(typeof(TrackerButton)))
                if (trackerManager.TrackerButtonPress(TrackerId, button))
                    ID.text = button.ToString();

            SendOSCMessage();
        }
        else
            Canvas.SetActive(false);
    }

    private void SendOSCMessage()
    {
        InitTracker.oscClient.Send(OSCAddress + "/Position", transform.position.x, transform.position.y, transform.position.z);
        InitTracker.oscClient.Send(OSCAddress + "/Rotation", transform.rotation.w, transform.rotation.x, transform.rotation.y, transform.rotation.z);
        InitTracker.oscClient.Send(OSCAddress + "/Battery", trackerManager.GetTrackerBatteryLife(TrackerId));
    }
}
