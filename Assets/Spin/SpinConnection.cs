using System;

namespace Brussels.Crew.Spin
{

    [Serializable]
    public class SpinConnection
    {
        public int port;
        public string host;
        public SpinConnection(int port, string host)
        {
            this.port = port;
            this.host = host;
        }
    }

}