using System.Net;

namespace Orion
{
    public struct NetworkEventArgs
    {
        #region Fields
        public readonly Ipv4EndPoint Host;
        public readonly byte[] Data;
        #endregion

        #region Constructors
        public NetworkEventArgs(Ipv4EndPoint host, byte[] data)
        {
            Argument.EnsureNotNull(data, "data");
            Host = host;
            Data = data;
        }
        #endregion
    }
}
