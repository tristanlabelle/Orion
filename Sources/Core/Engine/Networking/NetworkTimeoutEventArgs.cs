using System.Net;

namespace Orion.Engine.Networking
{
    public struct NetworkTimeoutEventArgs
    {
        #region Fields
        public readonly IPv4EndPoint Host;
        public readonly byte[] Data;
        #endregion

        #region Constructors
        public NetworkTimeoutEventArgs(IPv4EndPoint host, byte[] data)
        {
            Host = host;
            Data = data;
        }
        #endregion
    }
}
