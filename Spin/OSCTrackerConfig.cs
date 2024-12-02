using System;
using System.Collections.Generic;
//using Base;
using OscJack;

namespace Brussels.Crew.Spin.Spin
{
    /// <summary>
    /// OSCTrackerConfig is a configuration class utilized within the Brussels.Crew.Spin.Spin namespace to manage and store settings
    /// for Open Sound Control (OSC) trackers. This includes configuration details for OSC device interaction,
    /// such as the device name, refresh rate, server connections, tracker roles, and tracker identifiers.
    /// </summary>
    /// <remarks>
    /// The class contains several key properties:
    /// - OSCDeviceName: A string representing the name of the OSC device.
    /// - OSCRefreshRate: An integer specifying the refresh rate for OSC communication, initialized to 120.
    /// - Servers: A list of SpinConnection objects representing server connections utilized by trackers.
    /// - TrackersRoles: A list of SpinRole objects detailing different roles assigned to trackers.
    /// - TrackerIds: An array of TrackerConfig, with a fixed size of 16, for storing individual tracker configurations.
    /// Additionally, there is a transient list:
    /// - oscClients: A non-serialized list of OscClient instances used for managing OSC network communication.
    /// The primary use of this class is to support application configurations involving multiple OSC tracking devices and their interactions.
    /// </remarks>
    [Serializable]
    public class OSCTrackerConfig
    {
        /// <summary>
        /// Represents the name of the OSC device being used in the configuration.
        /// </summary>
        /// <remarks>
        /// OSCDeviceName is initialized based on the system's device name and serves as a unique identifier
        /// for communication with other OSC devices. It is used within the context of the OSCTrackerConfig
        /// to manage and identify connected devices.
        /// </remarks>
        public string OSCDeviceName;

        /// <summary>
        /// Specifies the number of times per second that the system will attempt to send OSC (Open Sound Control) messages
        /// to configured devices or clients. A higher refresh rate ensures more frequent updates to the connected devices,
        /// which can be important for applications requiring real-time feedback or control. The default value is set to 120
        /// updates per second, but this rate can be adjusted as needed based on the requirements of the application and
        /// performance considerations.
        /// </summary>
        public int OSCRefreshRate = 120;

        /// <summary>
        /// Represents a collection of OSC server connections used within the application.
        /// </summary>
        /// <remarks>
        /// Each server connection is defined by a <see cref="SpinConnection"/> object that specifies the host, port, and name.
        /// This list is utilized for configuring connections to various OSC servers.
        /// </remarks>
        public List<SpinConnection> Servers = new List<SpinConnection>();

        /// <summary>
        /// Represents a collection of roles configured for OSC (Open Sound Control) trackers within the system.
        /// Each role in the collection is represented by a <see cref="SpinRole"/> object, which includes
        /// details such as name, address, activity status, associated servers, and the specific tracker it pertains to.
        /// This list is utilized to manage and interact with different roles assigned to various trackers.
        /// </summary>
        public List<SpinRole> TrackersRoles = new List<SpinRole>();

        /// <summary>
        /// An array of <see cref="TrackerConfig"/> objects representing individual tracker configurations in the OSC tracker system.
        /// </summary>
        /// <remarks>
        /// This array is initialized with a default size of 16. Each element corresponds to a unique tracker configuration within the system.
        /// The elements in this array may be resized dynamically to accommodate additional tracker configurations as needed.
        /// If an element is null or contains an empty <see cref="TrackerConfig.Name"/>, it can be initialized with a new <see cref="TrackerConfig"/> instance.
        /// </remarks>
        public TrackerConfig[] TrackerIds = new TrackerConfig[16];

        /// <summary>
        /// A non-serialized list of OscClient objects used to manage Open Sound Control (OSC) client connections.
        /// This list is dynamically populated and managed at runtime to establish and maintain communication with
        /// OSC servers defined in an application's configuration. Each client in the list corresponds to an individual
        /// OSC server connection, allowing the application to send data using the OSC protocol. The lifespan of these
        /// clients is tied to the application's need to interact with the servers, being actively created and disposed of
        /// as needed to optimize resource usage and ensure proper network communication handling.
        /// </summary>
        [NonSerialized] public List<OscClient> oscClients = new List<OscClient>();
    }

}