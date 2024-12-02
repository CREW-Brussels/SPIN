using System;
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
    /// The Tracker class is responsible for initializing and managing the tracking information
    /// for various device types within a Unity environment. This class interacts with the
    /// SpinConfigManager and TrackerManager to configure and update tracker settings.
    public class Tracker : MonoBehaviour
    {
        /// <summary>
        /// Represents the canvas GameObject used for UI display purposes in the Tracker.
        /// </summary>
        public GameObject Canvas;

        /// <summary>
        /// Represents a Tracker object within the Spin system, responsible for initializing and managing tracker devices.
        /// </summary>
        /// <remarks>
        /// The Tracker class utilizes the SpinConfigManager and TrackerManager to configure and track device connections
        /// as well as OSC address management related to trackers within the system. It allows initialization either by
        /// device type or by unique tracker ID.
        /// </remarks>
        public TMP_Text Name;

        /// Represents the ID of a tracker using TextMesh Pro's TMP_Text component.
        /// This field is used to display the identifier of a tracker device within the user interface.
        public TMP_Text ID;

        /// <summary>
        /// The Address field in the Tracker class refers to a TMPro Text component that displays the OSC address
        /// corresponding to a particular tracker device. It is updated based on the role and configuration of the tracker.
        /// </summary>
        public TMP_Text Address;

        /// A MonoBehaviour class responsible for tracking and managing tracker devices.
        /// It initializes the trackers, updates their state, and handles configuration updates.
        /// The class also manages display elements within a Unity canvas and updates UI text elements with tracker information and status.
        public TMP_Text Status;

        /// Represents the battery information display component for the Tracker.
        /// This text field updates to reflect the current battery status of the Tracker device.
        public TMP_Text Bat;

        #if UNITY_EDITOR
        /// <summary>
        /// The Tracker class is responsible for managing the state and functionality of a tracking device within the Spin system.
        /// </summary>
        [FormerlySerializedAs("Debug")] public bool debug = false;
        #endif

        /// <summary>
        /// Represents the Role property that determines the role index of the tracker within
        /// the configuration, used to identify the tracker's specific role based on its ID.
        /// </summary>
        /// <remarks>
        /// The role index is determined by matching the tracker's ID with the roles configured
        /// in `OSCTrackersConfig`. If a match is not found, the role defaults to -1.
        /// </remarks>
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

        /// Represents the current role of a tracker in the system.
        /// It is used to identify the role of the tracker by its ID within the tracker's configuration.
        /// If the role has not been set, it is initialized by checking the SpinConfigManager's list of tracker roles.
        /// The role index is returned, which corresponds to the tracker's position in the configuration.
        /// A value of -1 indicates that the role has not been assigned or found in the configuration.
        private int _role = -1;

        /// <summary>
        /// Represents the connection status of the tracker.
        /// </summary>
        /// <remarks>
        /// This variable is used to indicate whether the tracker is currently connected or not.
        /// The connection status affects the configuration update events within the system.
        /// </remarks>
        private bool _connected = false;

        /// <summary>
        /// Represents the main camera in the scene, which is used to determine
        /// the position and orientation of game objects relative to the user's viewpoint.
        /// </summary>
        private Camera _camera;

        /// <summary>
        /// Stores the OSC (Open Sound Control) address used for sending
        /// messages to specific endpoints within the tracking system.
        /// </summary>
        private string _oscAddress;

        /// <summary>
        /// Represents an instance of the TrackerManager class, responsible for handling tracking operations
        /// such as retrieving the tracking state, position, and rotation of a tracker device.
        /// This variable is utilized for managing tracker-specific functionalities within the Tracker component.
        /// </summary>
        private TrackerManager _trackerManager;

        /// <summary>
        /// Represents the unique identifier for a tracker instance.
        /// </summary>
        /// <remarks>
        /// The <c>_trackerId</c> is used to associate a specific tracker with its configuration
        /// and status updates within the tracking system. It serves as an index for
        /// accessing the tracker's settings in related configuration classes.
        /// </remarks>
        private int _trackerId;

        /// <summary>
        /// The _spinConfigManager is a private instance of the SpinConfigManager class,
        /// responsible for managing configuration settings related to the spin trackers.
        /// It is used to access and update tracker configurations within the Tracker class.
        /// </summary>
        private SpinConfigManager _spinConfigManager;

        /// <summary>
        /// Represents the type of the device that this tracker instance is associated with.
        /// It is utilized to determine the nature of the device being tracked,
        /// such as a head-mounted display or controller. This variable plays a key role
        /// in identifying input configurations, handling tracking states,
        /// and distinguishing between different devices within the Trackers system.
        /// </summary>
        private WVR_DeviceType _deviceType = WVR_DeviceType.WVR_DeviceType_Invalid;

        /// <summary>
        /// Initializes the camera reference as the main camera in the scene. This method is automatically called when the
        /// script instance is being loaded. It's used to set up the initial state of the tracker component, especially
        /// setting the private camera field to the main camera available in the scene at the time of loading.
        /// </summary>
        private void Start()
        {
            _camera = Camera.main;
        }

        /// Initializes the tracker with the specified device type and identifier.
        /// This method sets internal variables for device type and tracker ID,
        /// and subscribes to configuration update events.
        /// It updates the tracker name display based on the device type,
        /// sets the name in the configuration, updates the OSC address, and saves
        /// the spin configuration.
        /// Parameters:
        /// deviceType: The type of the device to initialize the tracker with.
        /// id: The identifier used for the tracker within the configuration.
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

        /// Initializes the tracker with the specified tracker ID.
        /// <param name="trackerId">The tracker ID used to initialize the tracker instance.</param>
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

        /// A method that handles configuration update events triggered by the SpinConfigManager.
        /// This method is primarily responsible for resetting the internal role state and
        /// updating the OSC address based on potentially new configuration data.
        private void ConfigUpdateEvent()
        {
            _role = -1;
            UpdateOscAddress();
        }

        /// Updates the Open Sound Control (OSC) address for the tracker based on its role and optional address parameter.
        /// <param name="adr">
        /// Optional parameter representing a custom address string. If provided, it overrides the default construction using
        /// the device name and role configuration from the SpinConfigManager. If null, the address will be constructed using
        /// the default settings.
        /// </param>
        /// <returns>
        /// The updated OSC address as a string. If the 'Role' is not set (-1), returns the default tracker ID as the address.
        /// </
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

        /// <summary>
        /// Gets or sets a value indicating whether the tracker information should be displayed or not.
        /// </summary>
        /// <remarks>
        /// When set to true, the associated Canvas GameObject is activated and the tracker's online status
        /// in the SpinConfigManager is updated accordingly. Conversely, when set to false, the Canvas is deactivated
        /// and the tracker's online status is set to offline.
        /// </remarks>
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

        /// <summary>
        /// Represents the visibility state of the informational display for the tracker.
        /// When set to true, the tracker canvas is active and the tracker's online status is updated.
        /// </summary>
        private bool _showInfo = true;

        /// <summary>
        /// Gets or sets the battery value of a tracker device.
        /// </summary>
        /// <remarks>
        /// The BatteryValue property represents the current battery level of the tracker device
        /// and updates the associated UI and configuration when changed. The value is a float,
        /// where the exact range and interpretation depend on the specific device and context.
        /// </remarks>
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

        /// <summary>
        /// Holds the current battery value of the tracker as a floating point percentage.
        /// This value is used for display purposes and to update the associated TrackerConfig
        /// in the OSCTrackersConfig with the battery status of the tracker identified by the tracker ID.
        /// </summary>
        private float _batteryValue;

        /// <summary>
        /// Represents the current tracking state of the tracker, indicating the presence of positional
        /// and rotational tracking data. This property monitors changes in the tracking state and updates
        /// the visual status feedback as well as the configuration settings accordingly.
        /// </summary>
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

        /// <summary>
        /// Represents the current tracking state of a tracker, indicating whether its position and/or rotation are being tracked.
        /// </summary>
        /// <remarks>
        /// The tracking state is used to update the visual status indicators within the UI and to determine the tracking capabilities of the tracker,
        /// such as whether it can track position or rotation.
        /// </remarks>
        private InputTrackingState _trackingState;

        /// Sends an OSC (Open Sound Control) message containing the current position, rotation, battery value,
        /// and tracking state of the tracker to the configured servers.
        /// <param name="ts">The current tracking state of the device.</param>
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

        /// <summary>
        /// Sends an OSC input message to the configured servers for the current tracker's role.
        /// </summary>
        /// <param name="button">The name of the button that triggered the message.</param>
        private void SendOscInputMessage(string button)
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
                        _spinConfigManager.OSCTrackersConfig.oscClients[server].Send(_oscAddress + "/" + button);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tracker is connected.
        /// Updates the corresponding tracker configuration to reflect the current connection status.
        /// </summary>
        /// <value>
        /// A boolean value where <c>true</c> indicates that the tracker is connected and <c>false</c> indicates it is not.
        /// </value>
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

        /// <summary>
        /// Adjusts the scale of the Canvas based on the distance from the camera to the object.
        /// The method calculates the distance between the camera and the object,
        /// and if the distance exceeds 2 units, it scales the Canvas proportionally.
        /// Otherwise, it applies a default small scale to the Canvas to ensure visibility
        /// regardless of the distance.
        /// </summary>
        void SetHidScale()
        {
            float camDist = Vector3.Distance(_camera.transform.position, transform.position);

            if (camDist > 2)
                Canvas.transform.localScale = Vector3.one * (camDist * 0.0005f);
            else
                Canvas.transform.localScale = Vector3.one * 0.001f;
        }

        /// <summary>
        /// Updates the tracker's state by scaling the canvas based on the distance
        /// to the camera and retrieving the current tracking information.
        /// </summary>
        /// <remarks>
        /// This method performs several tasks related to maintaining the up-to-date state
        /// of a tracker, such as adjusting tracker visuals, checking connectivity, updating
        /// its position, and optionally sending OSC messages if configured to do so.
        /// It also updates the displayed information based on the current tracking state.
        /// </remarks>
        void Update()
        {
            SetHidScale();
            if (TestConnection())
            {
                ShowInfo = true;
                UpdatePosition();
                SendButtonPressed();
                GetBatteryLevel();
                TrackingState = GetTrackingState();
                if (_spinConfigManager.Send)
                    SendOscMessage(TrackingState);
            }
            else
                ShowInfo = false;
        }

        /// <summary>
        /// Determines which input buttons are valid for the current device type, checks
        /// if any of those buttons are pressed, and sends an OSC message for each pressed button.
        /// </summary>
        private void SendButtonPressed()
        {
            WVR_InputId[] validInput;
            switch (_deviceType)
            {
                case WVR_DeviceType.WVR_DeviceType_HMD :
                    validInput = new WVR_InputId[]
                    {
                        WVR_InputId.WVR_InputId_Alias1_Enter
                    };
                    break;
                case WVR_DeviceType.WVR_DeviceType_Controller_Left:
                    validInput = new WVR_InputId[]
                    {
                        WVR_InputId.WVR_InputId_Alias1_Grip, 
                        WVR_InputId.WVR_InputId_Alias1_Trigger, 
                        WVR_InputId.WVR_InputId_Alias1_X, 
                        WVR_InputId.WVR_InputId_Alias1_Y, 
                        WVR_InputId.WVR_InputId_Alias1_Thumbstick
                    };
                    break;
                case WVR_DeviceType.WVR_DeviceType_Controller_Right:
                    validInput = new WVR_InputId[]
                    {
                        WVR_InputId.WVR_InputId_Alias1_Grip, 
                        WVR_InputId.WVR_InputId_Alias1_Trigger,
                        WVR_InputId.WVR_InputId_Alias1_A, 
                        WVR_InputId.WVR_InputId_Alias1_B,
                        WVR_InputId.WVR_InputId_Alias1_Thumbstick
                    };
                    break;
                default:
                    validInput = new WVR_InputId[] { };
                    break;
            }

            foreach (WVR_InputId _WVR_InputId in validInput)
            {
                if (WXRDevice.ButtonPress(_deviceType, _WVR_InputId))
                {
                    SendOscInputMessage(_WVR_InputId.ToString());
                }
            }
        }

        /// <summary>
        /// Retrieves the current tracking state of the tracker based on the device type or tracker ID.
        /// </summary>
        /// <returns>The current <see cref="InputTrackingState"/> of the tracker, indicating whether it is being tracked or not.</returns>
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

        /// <summary>
        /// Retrieves the battery level of the device based on the specified device type.
        /// </summary>
        /// <remarks>
        /// Depending on the current device type (_deviceType), this method will determine the appropriate way to fetch the battery level.
        /// If the device type is invalid, it will fetch the battery life using the tracker ID. For other device types such as HMD and controllers,
        /// it uses the InputDeviceControl to obtain the battery level for the head, left controller, or right controller, respectively.
        /// The result is stored in the BatteryValue property.
        /// </remarks>
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

        /// <summary>
        /// Updates the position and rotation of the tracker based on the device type.
        /// </summary>
        /// <remarks>
        /// The method checks the device type and updates the tracker's position and
        /// rotation accordingly. For head-mounted displays (HMD), it uses the camera's
        /// current position and rotation. For invalid device types, it retrieves the
        /// position and rotation from the tracker manager. For other valid device types,
        /// it retrieves the data from the Wave Essence instance. In Unity Editor mode, if
        /// debugging is enabled, it uses predefined fake position and rotation values.
        /// The position update only occurs if the calculated position is non-zero.
        /// </remarks>
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

        /// <summary>
        /// Tests the connection status of a tracker device. Determines if the tracker is currently connected based on its device type.
        /// </summary>
        /// <returns>True if the tracker is connected; otherwise, false.</returns>
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
        /// Generates a fake rotation for debugging purposes.
        /// This method simulates a rotation by incrementally rotating around the X-axis based on the current frame count.
        /// <returns>A Quaternion representing a simulated rotation.</returns>
        private Quaternion GetFakeRotation()
        {
            return Quaternion.Euler(.1f * Time.frameCount % 360, 0f, 0f);
        }

        /// <summary>
        /// Generates a fake position vector based on the cosine and sine of the frame count,
        /// allowing for a simulated movement in two dimensions over time.
        /// </summary>
        /// <returns>
        /// A Vector3 representing a fake position calculated using trigonometric functions
        /// based on the current frame count. The resulting vector offers a simulation of
        /// a cyclic movement pattern.
        /// </returns>
        private Vector3 GetFakePosition()
        {
            return new Vector3(Mathf.Cos(.1f * Time.frameCount), Mathf.Sin(.1f * Time.frameCount));
        }
#endif

        /// <summary>
        /// Called when the object is being destroyed. Unsubscribes from the
        /// ConfigUpdatedEvent to clean up event handlers and prevent potential
        /// memory leaks or unintended behavior.
        /// </summary>
        private void OnDestroy()
        {
            _spinConfigManager.ConfigUpdatedEvent -= ConfigUpdateEvent;
        }
    }
}