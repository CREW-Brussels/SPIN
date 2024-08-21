using System;

namespace Brussels.Crew.Spin
{

    [Serializable]
    public class SpinConnection
    {
        public int port;
        public string host;
        public string name;
        public SpinConnection(int port, string host, string name)
        {
            this.port = port;
            this.host = host;
            this.name = name;
        }
    }

}