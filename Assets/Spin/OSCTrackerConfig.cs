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
        public List<SpinRole> TrackersRoles = new List<SpinRole>();
        public TrackerConfig[] TrackerIds = new TrackerConfig[16];

        [NonSerialized] public List<OscClient> oscClients = new List<OscClient>();
    }

}