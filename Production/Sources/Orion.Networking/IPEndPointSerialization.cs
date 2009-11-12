using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Orion.Networking
{
    internal static class IPEndPointSerialization
    {
        public static void CopyTo(this IPEndPoint endpoint, byte[] array, long index)
        {
            byte[] addressBytes = endpoint.Address.GetAddressBytes();
            addressBytes.CopyTo(array, index);
            ushort port = (ushort)endpoint.Port;
            array[index + addressBytes.Length] = (byte)(port & 0xFF);
            array[index + addressBytes.Length + 1] = (byte)(port >> 8);
        }

        public static IPEndPoint Deserialize(byte[] array, int index)
        {
            long address = BitConverter.ToInt32(array, index);
            int port = BitConverter.ToUInt16(array, index + 4);
            return new IPEndPoint(new IPAddress(address), port);
        }
    }
}
