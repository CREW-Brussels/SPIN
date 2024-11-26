using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using Wave.Essence;
using Wave.Essence.Tracker;
using Wave.Native;
using Wave.OpenXR;

namespace Brussels.Crew.Spin.Spin
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
        [FormerlySerializedAs("Debug")] public bool debug = false;
        #endif

        private int Role
        {
            get
            {
                if (_role == -1 && _spinConfigManager)
                {
                    for (int i = 0; i < _spinConfigManager.OSCTrackersConfig.TrackersRoles.Count; i++)
                        if (_spinConfigManager.OSCTrackersConfig.TrackersRoles[i].tracker == _trackerId)
                            _role = i;
                }
                return _role;
            }
        }
        private int _role = -1;

        private bool _connected = false;
        private Camera _camera;
        private string _oscAddress;
        private TrackerManager _trackerManager;
        private int _trackerId;
        private SpinConfigManager _spinConfigManager;
        private WVR_DeviceType _deviceType = WVR_DeviceType.WVR_DeviceType_Invalid;

        private void Start()
        {
            _camera = Camera.main;
        }

        public void Init(WVR_DeviceType deviceType, int id)
        {
            _trackerId = id;
            _deviceType = deviceType;
            _spinConfigManager = SpinConfigManager.Instance;
            _trackerManager = TrackerManager.Instance;

            _spinConfigManager.ConfigUpdatedEvent += ConfigUpdateEvent;

            if (ID != null)
                ID.text = deviceType.ToString();

            string nameText =  deviceType.ToString();
            
            string textToTrim = "WVR_DeviceType_";

            if (nameText.StartsWith(textToTrim))
                nameText = nameText.Remove(0, textToTrim.Length);

            if (Name != null)
                Name.text = nameText;

            _spinConfigManager.OSCTrackersConfig.TrackerIds[id].Name = nameText;

            UpdateOscAddress();

            SpinConfigManager.Instance.SaveSpinConfig();        }
        
        public void Init(TrackerId trackerId)
        {
            _deviceType = WVR_DeviceType.WVR_DeviceType_Invalid;
            _trackerId = (int)trackerId;

            _spinConfigManager = SpinConfigManager.Instance;
            _trackerManager = TrackerManager.Instance;

            _spinConfigManager.ConfigUpdatedEvent += ConfigUpdateEvent;

            if (ID != null)
                ID.text = trackerId.ToString();

            string nameText =  trackerId.ToString();

            if (Name != null)
                Name.text = nameText;

            _spinConfigManager.OSCTrackersConfig.TrackerIds[(int)trackerId].Name = nameText;

            UpdateOscAddress();

            SpinConfigManager.Instance.SaveSpinConfig();
        }

        private void ConfigUpdateEvent()
        {
            _role = -1;
            UpdateOscAddress();
        }

        private string UpdateOscAddress(string adr = null)
        {
            if (Role == -1)
                return "/" + _trackerId.ToString();

            if (adr == null)
            {
                if (_spinConfigManager.OSCTrackersConfig.OSCDeviceName == null)
                    _spinConfigManager.OSCTrackersConfig.OSCDeviceName = "Spin";
                _oscAddress = "/" + _spinConfigManager.OSCTrackersConfig.OSCDeviceName.Trim('/').Trim() + "/" + _spinConfigManager.OSCTrackersConfig.TrackersRoles[Role].address.Trim('/').Trim();
            }
            else
                _oscAddress = adr;

            if (Address)
                Address.text = _oscAddress;

            return _oscAddress;
        }

        private bool ShowInfo
        {
            get => _showInfo;
            set
            {
                if (_showInfo != value)
                {
                    _showInfo = value;
                    if (Canvas)
                        Canvas.SetActive(value);
                    _spinConfigManager.OSCTrackersConfig.TrackerIds[(int)_trackerId].Online = value;
                }
            }
        }
        private bool _showInfo = true;

        private float BatteryValue
        {
            get => _batteryValue;
            set
            {
                if (!Mathf.Approximately(_batteryValue, value))
                {
                    _batteryValue = value;
                    if (Bat)
                        Bat.text = "Battery " + value.ToString("P1", CultureInfo.InvariantCulture);
                    _spinConfigManager.OSCTrackersConfig.TrackerIds[(int)_trackerId].Battery = value;
                }
            }
        }
        private float _batteryValue;

        private InputTrackingState TrackingState
        {
            get => _trackingState;
            set
            {
                if (_trackingState != value)
                {
                    _trackingState = value;
                    if (Status)
                    {
                        string pos = (value & InputTrackingState.Position) != 0 ? "green" : "red";
                        string rot = (value & InputTrackingState.Rotation) != 0 ? "green" : "red";

                        Status.richText = true;
                        Status.text = $"<color=\"{pos}\">Position <color=\"{rot}\">Rotation";
                    }
                    _spinConfigManager.OSCTrackersConfig.TrackerIds[(int)_trackerId].TrackingPosition = (value & InputTrackingState.Position) != 0;
                    _spinConfigManager.OSCTrackersConfig.TrackerIds[(int)_trackerId].TrackingRotation = (value & InputTrackingState.Rotation) != 0;
                }
            }
        }
        private InputTrackingState _trackingState;

        private void SendOscMessage(InputTrackingState ts)
        {
            if (Role == -1)
                return;

            #if UNITY_EDITOR
            if (debug)
                _spinConfigManager.OSCTrackersConfig.TrackersRoles[Role].active = true;
            #endif

            if (_spinConfigManager.OSCTrackersConfig.TrackersRoles[Role].active)
            {
                foreach (int server in _spinConfigManager.OSCTrackersConfig.TrackersRoles[Role].servers)
                {
                    if (_spinConfigManager.OSCTrackersConfig.oscClients.Count >= server && _spinConfigManager.OSCTrackersConfig.oscClients[server] != null)
                    {
                        _spinConfigManager.OSCTrackersConfig.oscClients[server].SendSpinMessage(_oscAddress,
                            transform.position.x,
                            transform.position.y,
                            transform.position.z,
                            transform.rotation.w,
                            transform.rotation.x,
                            transform.rotation.y,
                            transform.rotation.z,
                            BatteryValue,
                            (uint)ts
                        );
                    }
                }
            }
        }

        private bool Connected
        {
            get => _connected;
            set
            {
                if (_connected != value)
                    _spinConfigManager.OSCTrackersConfig.TrackerIds[(int)_trackerId].Online = value;
                _connected = value;
            }
        }

        void SetHidScale()
        {
            float camDist = Vector3.Distance(_camera.transform.position, transform.position);

            if (camDist > 2)
                Canvas.transform.localScale = Vector3.one * (camDist * 0.0005f);
            else
                Canvas.transform.localScale = Vector3.one * 0.001f;
        }
        
        void Update()
        {
            SetHidScale();
            if (TestConnection())
            {
                ShowInfo = true;
                UpdatePosition();
                GetBatteryLevel();
                TrackingState = GetTrackingState();
                if (_spinConfigManager.Send)
                    SendOscMessage(TrackingState);
            }
            else
                ShowInfo = false;
        }

        private InputTrackingState GetTrackingState()
        {
            InputTrackingState TS;
            if (_deviceType == WVR_DeviceType.WVR_DeviceType_Invalid)
                _trackerManager.GetTrackerTrackingState((TrackerId)_trackerId, out TS);
            else
                TS = WaveEssence.Instance.IsTracked(_deviceType) ? InputTrackingState.All : InputTrackingState.None;

            TrackingState = TS;
            return TS;
        }

        private void GetBatteryLevel()
        {
            if (_deviceType == WVR_DeviceType.WVR_DeviceType_Invalid)
                BatteryValue = _trackerManager.GetTrackerBatteryLife((TrackerId)_trackerId);
            else if (_deviceType == WVR_DeviceType.WVR_DeviceType_HMD)
                BatteryValue = InputDeviceControl.GetBatteryLevel(InputDeviceControl.ControlDevice.Head);
            else if (_deviceType == WVR_DeviceType.WVR_DeviceType_Controller_Left)
                BatteryValue = InputDeviceControl.GetBatteryLevel(InputDeviceControl.ControlDevice.Left);
            else if (_deviceType == WVR_DeviceType.WVR_DeviceType_Controller_Right)
                BatteryValue = InputDeviceControl.GetBatteryLevel(InputDeviceControl.ControlDevice.Right);
        }

        private void UpdatePosition()
        {
            Vector3 pos;
            Quaternion rot;
            if (_deviceType == WVR_DeviceType.WVR_DeviceType_HMD)
            {
                pos = _camera.transform.position;
                rot = _camera.transform.rotation;
            }
            else if (_deviceType == WVR_DeviceType.WVR_DeviceType_Invalid)
            {
                pos = _trackerManager.GetTrackerPosition((TrackerId)_trackerId);
                rot = _trackerManager.GetTrackerRotation((TrackerId)_trackerId);
            }
            else
            {
                pos = WaveEssence.Instance.GetDevicePosition(_deviceType);
                rot = WaveEssence.Instance.GetDeviceRotation(_deviceType);
            }
#if UNITY_EDITOR
            if (debug)
            {
                pos = GetFakePosition();
                rot = GetFakeRotation();
            }
#endif
            if (pos != Vector3.zero)
            {
                transform.position = pos;
                transform.rotation = rot;
            }
        }

        private bool TestConnection()
        {
            try
            {
                if (_deviceType == WVR_DeviceType.WVR_DeviceType_Invalid)
                    Connected = _trackerManager.IsTrackerConnected((TrackerId)_trackerId);
                else if (_deviceType == WVR_DeviceType.WVR_DeviceType_HMD)
                    Connected = true;
                else
                    Connected = WaveEssence.Instance.IsConnected(_deviceType);
            }
            catch
            {
                Connected = false;
            }

#if UNITY_EDITOR
            if (debug)
                Connected = true;
#endif
            return Connected;
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
            _spinConfigManager.ConfigUpdatedEvent -= ConfigUpdateEvent;
        }
    }
}