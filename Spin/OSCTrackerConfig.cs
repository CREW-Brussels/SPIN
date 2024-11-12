using System;
using System.Collections.Generic;
//using Base;
using OscJack;

namespace Brussels.Crew.Spin.Spin
{

    [Serializable]
    public class OSCTrackerConfig
    {
        public string OSCDeviceName;
        public int OSCRefreshRate = 120;
        public List<SpinConnection> Servers = new List<SpinConnection>();
        public List<SpinRole> TrackersRoles = new List<SpinRole>();
        public TrackerConfig[] TrackerIds = new TrackerConfig[16];

        [NonSerialized] public List<OscClient> oscClients = new List<OscClient>();
    }

}