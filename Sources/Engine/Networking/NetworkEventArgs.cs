using System.Net;

namespace Orion.Engine.Networking
{
    public struct NetworkEventArgs
    {
        #region Fields
        public readonly IPv4EndPoint Host;
        public readonly byte[] Data;
        #endregion

        #region Constructors
        public NetworkEventArgs(IPv4EndPoint host, byte[] data)
        {
            Argument.EnsureNotNull(data, "data");
            Host = host;
            Data = data;
        }
        #endregion
    }
}
