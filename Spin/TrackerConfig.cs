using System;

namespace Brussels.Crew.Spin.Spin
{
    /// <summary>
    /// Represents the configuration for an individual tracker device.
    /// </summary>
    /// <remarks>
    /// This class is used to manage various properties and state information related to a tracker device including its name,
    /// battery status, online status, and whether it's tracking position and rotation. It also contains a reference to an
    /// actual tracker object, which is not serialized.
    /// </remarks>
    [Serializable]
    public class TrackerConfig
    {
        /// <summary>
        /// Represents the name of a tracker in the TrackerConfig class.
        /// </summary>
        /// <remarks>
        /// This variable is used to store the human-readable name or identifier for a tracker device.
        /// It is utilized in the initialization and configuration of tracker instances within the application.
        /// </remarks>
        public string Name;


        /// <summary>
        /// Represents an instance of the Tracker associated with the TrackerConfig.
        /// </summary>
        /// <remarks>
        /// This field is used to maintain a reference to the Tracker component,
        /// which provides functionality to manage the display and status of trackers
        /// within an assigned canvas in the application.
        /// It is crucial for associating and managing specific trackers in the OSCTrackerManager
        /// to ensure the appropriate instances are initialized and updated accordingly.
        /// </remarks>
        [NonSerialized] public Tracker tracker;

        /// <summary>
        /// Represents the current battery charge level of the tracker device as a floating point percentage.
        /// This value is monitored to assess the remaining power supply of the tracker, influencing its operating duration.
        /// It is updated and utilized for both display purposes and configuration management within the system.
        /// </summary>
        public float Battery;

        /// <summary>
        /// Indicates whether the tracker is currently online.
        /// This property is used to represent the online status of a tracker device,
        /// determining if it is connected and active within the system.
        /// </summary>
        public bool Online;

         /// <summary>
         /// Indicates whether the tracker is actively tracking the position.
         /// </summary>
         public bool TrackingPosition;

         /// <summary>
         /// Indicates whether the tracker's rotation is currently being tracked.
         /// </summary>
         public bool TrackingRotation;
    }

}