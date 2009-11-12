using System.Net;

namespace Orion
{
    public struct NetworkTimeoutEventArgs
    {
        #region Fields
        public readonly Ipv4EndPoint Host;
        public readonly byte[] Data;
        #endregion

        #region Constructors
        public NetworkTimeoutEventArgs(Ipv4EndPoint host, byte[] data)
        {
            Host = host;
            Data = data;
        }
        #endregion
    }
}
