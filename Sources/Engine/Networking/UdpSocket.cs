using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;

namespace Orion.Engine.Networking
{
    /// <summary>
    /// Provides methods to send and receive UDP datagrams.
    /// Derived classes are expected to be thread safe.
    /// </summary>
    public abstract class UdpSocket : IDisposable
    {
        #region Fields
        private bool isDisposed;
        #endregion

        #region Finalizer
        ~UdpSocket()
        {
            Dispose(false);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the local UDP end point to which this socket is bound.
        /// </summary>
        public abstract IPv4EndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the IPv4 address to which this socket is bound.
        /// </summary>
        public IPv4Address LocalAddress
        {
            get { return LocalEndPoint.Address; }
        }

        /// <summary>
        /// Gets the UDP port to which this socket is bound.
        /// </summary>
        public ushort LocalPort
        {
            get { return LocalEndPoint.Port; }
        }

        /// <summary>
        /// Gets the length of data ready to be received.
        /// A value of <c>0</c> indicates that a call to <see cref="Receive"/> will block until data has arrived.
        /// </summary>
        public abstract int AvailableDataLength { get; }

        /// <summary>
        /// Gets a value indicating if data is ready to be received.
        /// </summary>
        public bool IsDataAvailable
        {
            get { return AvailableDataLength > 0; }
        }

        /// <summary>
        /// Gets a value indicating if this socket has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return isDisposed; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sends an UDP datagram to a remote host.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        /// <param name="endPoint">The host to which the data is to be sent.</param>
        public abstract void Send(Subarray<byte> data, IPv4EndPoint endPoint);

        /// <summary>
        /// Receives an UDP datagram from a remote host.
        /// This call blocks unless data is available.
        /// </summary>
        /// <param name="data">A buffer to receive the data.</param>
        /// <param name="endPoint">Outputs the UDP/IP end point of the sender.</param>
        public abstract void Receive(Subarray<byte> data, out IPv4EndPoint endPoint);

        /// <summary>
        /// Broadcasts an UDP datagram on a given port.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        /// <param name="port">The port on which broadcasting should be done.</param>
        public virtual void Broadcast(Subarray<byte> data, ushort port)
        {
            Send(data, new IPv4EndPoint(IPv4Address.Broadcast, port));
        }

        protected void EnsureNotDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(null);
        }

        /// <summary>
        /// Releases all resources used by this socket.
        /// </summary>
        public void Close()
        {
            EnsureNotDisposed();

            try
            {
                Dispose(true);
            }
            finally
            {
                isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing) { }

        void IDisposable.Dispose()
        {
            Close();
        }
        #endregion
    }
}
