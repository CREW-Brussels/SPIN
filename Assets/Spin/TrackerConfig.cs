using System.Collections.Generic;
using System;

namespace Brussels.Crew.Spin
{

    [Serializable]
    public class TrackerConfig
    {
        public List<int> Servers;
        public int Role;
        public string Name;

        [NonSerialized] public Tracker tracker;
        [NonSerialized] public float Battery;
        [NonSerialized] public bool Active;
        [NonSerialized] public bool Online;
        [NonSerialized] public bool TrackingPosition;
        [NonSerialized] public bool TrackingRotation;
    }

}