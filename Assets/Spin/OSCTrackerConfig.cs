using OscJack;
using System.Collections.Generic;
using System;

namespace Brussels.Crew.Spin
{

    [Serializable]
    public class OSCTrackerConfig
    {
        public string OSCDeviceName;
        public List<SpinConnection> Servers = new List<SpinConnection>();
        public List<string> TrackersRoles = new List<string>();
        public TrackerConfig[] TrackerIds = new TrackerConfig[16];

        [NonSerialized] public List<OscClient> oscClients = new List<OscClient>();
    }

}