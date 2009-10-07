using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion
{
    public struct NetworkEventArgs
    {
        public readonly IPEndPoint Host;
        public readonly byte[] Data;

        public NetworkEventArgs(IPEndPoint host, byte[] data)
        {
            Host = host;
            Data = data;
        }
    }
}
