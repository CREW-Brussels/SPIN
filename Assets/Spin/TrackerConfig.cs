using System.Collections.Generic;
using System;

namespace Brussels.Crew.Spin
{

    [Serializable]
    public class TrackerConfig
    {
        public string Name;

        
        [NonSerialized] public Tracker tracker;
         public float Battery;
         public bool Online;
         public bool TrackingPosition;
         public bool TrackingRotation;
    }

}