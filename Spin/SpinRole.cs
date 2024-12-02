using System;
using System.Collections.Generic;
using Wave.Essence.Tracker;

namespace Brussels.Crew.Spin.Spin
{
    /// <summary>
    /// Represents a role used within the spin system, encompassing details pertinent to its function
    /// and configuration within the network.
    /// </summary>
    [Serializable]
    public class SpinRole
    {
        /// <summary>
        /// Represents the name of the SpinRole.
        /// </summary>
        public string name;

        /// <summary>
        /// Represents the network address or location associated with a specific role in the Spin system.
        /// </summary>
        public string address;

        /// <summary>
        /// Indicates whether the SpinRole is active or inactive.
        /// </summary>
        public bool active;

        /// <summary>
        /// Represents a list of server identifiers that are associated with a particular SpinRole.
        /// These servers are utilized for handling specific tracking tasks within the application.
        /// The integers in the list reference server instances configured within the application,
        /// allowing for dynamic assignment and management of server roles.
        /// </summary>
        public List<int> servers;

        /// <summary>
        /// Represents an identifier for tracking within a SpinRole.
        /// This variable is utilized to correlate the spin role in configuration tracking,
        /// ensuring the correct association and management of roles within the broader system.
        /// </summary>
        public int tracker;
    }
}
