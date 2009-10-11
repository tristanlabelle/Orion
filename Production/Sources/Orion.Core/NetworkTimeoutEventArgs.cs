using System;
using System.Net;

namespace Orion
{
	public struct NetworkTimeoutEventArgs
	{
		public readonly IPEndPoint Host;
		public readonly byte[] Data;
		
		public NetworkTimeoutEventArgs(IPEndPoint host, byte[] data)
		{
			Host = host;
			Data = data;
		}
	}
}
