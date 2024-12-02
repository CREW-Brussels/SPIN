using System;

namespace Brussels.Crew.Spin.Spin
{
    /// <summary>
    /// Represents a network connection for the Spin system.
    /// </summary>
    /// <remarks>
    /// A SpinConnection includes details about the network port, the host address,
    /// and the connection name. Instances of this class are used to configure
    /// connections to remote services or servers.
    /// </remarks>
    [Serializable]
    public class SpinConnection
    {
        /// <summary>
        /// The port number used to establish a connection for data communication.
        /// </summary>
        public int port;

        /// <summary>
        /// Represents the hostname or IP address of the server for the OSC connection.
        /// This string is used to establish communication with the server in conjunction with a port.
        /// </summary>
        public string host;

        /// <summary>
        /// Represents the name associated with the Spin connection.
        /// </summary>
        public string name;

        /// <summary>
        /// Represents a connection with specified port, host, and name.
        /// </summary>
        public SpinConnection(int port, string host, string name)
        {
            this.port = port;
            this.host = host;
            this.name = name;
        }
    }

}