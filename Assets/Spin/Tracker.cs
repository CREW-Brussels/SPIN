using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using Wave.Essence.Tracker;
using Wave.Native;

namespace Brussels.Crew.Spin
{

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

        private int role
        {
            get
            {
                if (_role == -1 && spinConfigManager != null)
                {
                    for (int i = 0; i < spinConfigManager.OSCTrackersConfig.TrackersRoles.Count; i++)
                        if (spinConfigManager.OSCTrackersConfig.TrackersRoles[i].tracker == TrackerId)
                            _role = i;
                }
                return _role;
            }
        }
        private int _role = -1;

        private string OSCAddress;
        private TrackerManager trackerManager;
        private TrackerId TrackerId;
        private SpinConfigManager spinConfigManager;

        public void Init(TrackerId trackerId)
        {
            TrackerId = trackerId;

            spinConfigManager = SpinConfigManager.Instance;
            trackerManager = TrackerManager.Instance;

            spinConfigManager.ConfigUpdatedEvent += ConfigUpdateEvent;

            if (ID != null)
                ID.text = trackerId.ToString();

            string name = trackerManager.GetTrackerDeviceName(trackerId);
            if (string.IsNullOrEmpty(name))
                name = trackerId.ToString();

            if (Name != null)
                Name.text = name;

            spinConfigManager.OSCTrackersConfig.TrackerIds[(int)trackerId].Name = name;

            UpdateOSCAddress();

            SpinConfigManager.Instance.SaveSpinConfig();
        }

        private void ConfigUpdateEvent()
        {
            _role = -1;
            UpdateOSCAddress();
        }

        public string UpdateOSCAddress(string adr = null)
        {
            if (role == -1)
                return "/" + TrackerId.ToString();

            if (adr == null)
            {
                if (spinConfigManager.OSCTrackersConfig.OSCDeviceName == null)
                    spinConfigManager.OSCTrackersConfig.OSCDeviceName = "Spin";
                OSCAddress = "/" + spinConfigManager.OSCTrackersConfig.OSCDeviceName.Trim('/').Trim() + "/" + spinConfigManager.OSCTrackersConfig.TrackersRoles[role].address.Trim('/').Trim();
            }
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
                    spinConfigManager.OSCTrackersConfig.TrackerIds[(int)TrackerId].Online = value;
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
                    spinConfigManager.OSCTrackersConfig.TrackerIds[(int)TrackerId].Battery = value;
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
                    spinConfigManager.OSCTrackersConfig.TrackerIds[(int)TrackerId].TrackingPosition = (value & InputTrackingState.Position) != 0;
                    spinConfigManager.OSCTrackersConfig.TrackerIds[(int)TrackerId].TrackingRotation = (value & InputTrackingState.Rotation) != 0;
                }
            }
        }
        private InputTrackingState _TrackingState;

        private void SendOSCMessage()
        {
            if (role == -1)
                return;

            #if UNITY_EDITOR
            if (Debug)
                spinConfigManager.OSCTrackersConfig.TrackersRoles[role].active = true;
            #endif

            if (spinConfigManager.OSCTrackersConfig.TrackersRoles[role].active)
            {
                try
                {
                    foreach (int server in spinConfigManager.OSCTrackersConfig.TrackersRoles[role].servers)
                    {
                        if (spinConfigManager.OSCTrackersConfig.oscClients.Count >= server && spinConfigManager.OSCTrackersConfig.oscClients[server] != null)
                        {
                            spinConfigManager.OSCTrackersConfig.oscClients[server].Send(OSCAddress,
                                transform.position.x,
                                transform.position.y,
                                transform.position.z,
                                transform.rotation.w,
                                transform.rotation.x,
                                transform.rotation.y,
                                transform.rotation.z,
                                BatteryValue);
                        }
                    }
                }
                catch(Exception e)
                {
                    //print("Error " + e.Message);
                }
            }
        }



        void Update()
        {
            bool connected = trackerManager.IsTrackerConnected(TrackerId);

            #if UNITY_EDITOR
            if (Debug)
                connected = true;
            #endif

            if (connected)
            {
                ShowInfo = true;

                transform.position = trackerManager.GetTrackerPosition(TrackerId);
                transform.rotation = trackerManager.GetTrackerRotation(TrackerId);

#if UNITY_EDITOR
                if (Debug)
                {
                    transform.position = GetFakePosition();
                    transform.rotation = GetFakeRotation();
                }
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

}