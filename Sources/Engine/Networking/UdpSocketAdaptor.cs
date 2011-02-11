using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Orion.Engine.Collections;
using System.Net;
using System.Diagnostics;
using System.Threading;

namespace Orion.Engine.Networking
{
    /// <summary>
    /// Adapts a UDP <see cref="System.Net.Sockets.Socket"/> to the <see cref="UdpSocket"/> interface.
    /// </summary>
    public sealed class UdpSocketAdaptor : UdpSocket
    {
        #region Fields
        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Cache for <see cref="IPEndPoint"/> instances.
        /// </summary>
        private static volatile Dictionary<IPv4EndPoint, IPEndPoint> ipEndPointCache = new Dictionary<IPv4EndPoint, IPEndPoint>();

        [ThreadStatic]
        private static EndPoint senderEndPoint;

        private readonly Socket socket;
        private readonly IPv4EndPoint localEndPoint;
        #endregion

        #region Constructors
        public UdpSocketAdaptor(IPv4EndPoint localEndPoint)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(localEndPoint);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, (int)ReceiveTimeout.TotalMilliseconds);
                socket.MulticastLoopback = false;
            }
            catch
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                    socket.Close();
                    throw;
                }

                socket.Close();
                throw;
            }

            this.localEndPoint = (IPv4EndPoint)socket.LocalEndPoint;
        }

        public UdpSocketAdaptor(IPv4Address address, ushort localPort) : this(new IPv4EndPoint(address, localPort)) { }
        public UdpSocketAdaptor(ushort localPort) : this(new IPv4EndPoint(IPv4Address.Any, localPort)) { }
        #endregion

        #region Properties
        public override IPv4EndPoint LocalEndPoint
        {
            get { return localEndPoint; }
        }

        public override int AvailableDataLength
        {
            get { return socket.Available; }
        }
        #endregion

        #region Methods
        public override void Send(Subarray<byte> data, IPv4EndPoint endPoint)
        {
            if (data.Count == 0) return;

            IPEndPoint ipEndPoint = GetIPEndPoint(endPoint);
            int sentDataLength = socket.SendTo(data.Array, data.Offset, data.Count, SocketFlags.None, ipEndPoint);

            Debug.Assert(sentDataLength == data.Count, "Socket.SendTo sent a different amount of data than it was supposed to.");
        }

        public override void Receive(Subarray<byte> data, out IPv4EndPoint endPoint)
        {
            if (senderEndPoint == null) senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

            int receivedDataLength = socket.ReceiveFrom(data.Array, data.Offset, data.Count, SocketFlags.None, ref senderEndPoint);
            endPoint = (IPv4EndPoint)(IPEndPoint)senderEndPoint;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            base.Dispose(disposing);
        }

        private static IPEndPoint GetIPEndPoint(IPv4EndPoint endPoint)
        {
            var currentIPEndPointCache = ipEndPointCache;
            IPEndPoint ipEndPoint;
            if (currentIPEndPointCache.TryGetValue(endPoint, out ipEndPoint)) return ipEndPoint;

            ipEndPoint = endPoint;
            
            var newIPEndPointCache = new Dictionary<IPv4EndPoint, IPEndPoint>(currentIPEndPointCache);
            newIPEndPointCache.Add(endPoint, ipEndPoint);
            ipEndPointCache = newIPEndPointCache;

            return ipEndPoint;
        }
        #endregion
    }
}
