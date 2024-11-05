using System;
using System.Collections.Generic;
using Wave.Essence.Tracker;

namespace Brussels.Crew.Spin.Spin
{
[Serializable]
    public class SpinRole
    {
        public string name;
        public string address;
        public bool active;
        public List<int> servers;
        public TrackerId tracker;
    }
}
