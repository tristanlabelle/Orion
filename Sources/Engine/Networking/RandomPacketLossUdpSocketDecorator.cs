using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;

namespace Orion.Engine.Networking
{
    /// <summary>
    /// A decorator which simulates UDP packet loss.
    /// </summary>
    public sealed class RandomPacketLossUdpSocketDecorator : UdpSocket
    {
        #region Fields
        private static readonly byte[] dummyBuffer = new byte[0];

        private readonly UdpSocket socket;
        private readonly Random random;
        private readonly float sendLossProbability;
        private readonly float receiveLossProbability;
        private readonly object @lock = new object();
        private bool hasPacketPassedLossTest;
        #endregion

        #region Constructors
        public RandomPacketLossUdpSocketDecorator(UdpSocket socket, Random random, float sendLossProbability, float receiveLossProbability)
        {
            Argument.EnsureNotNull(socket, "socket");
            Argument.EnsureNotNull(random, "random");
            Argument.EnsureWithin(sendLossProbability, 0, 1, "sendLossProbability");
            Argument.EnsureWithin(receiveLossProbability, 0, 1, "receiveLossProbability");

            this.socket = socket;
            this.random = random;
            this.sendLossProbability = sendLossProbability;
            this.receiveLossProbability = receiveLossProbability;
        }

        public RandomPacketLossUdpSocketDecorator(UdpSocket socket, Random random, float lossProbability)
            : this(socket, random, lossProbability, lossProbability) { }
        #endregion

        #region Properties
        public override IPv4EndPoint LocalEndPoint
        {
            get { return socket.LocalEndPoint; }
        }

        public override int AvailableDataLength
        {
            get
            {
                if (receiveLossProbability == 0) return socket.AvailableDataLength;

                lock (@lock)
                {
                    int dataLength;
                    while (true)
                    {
                        dataLength = socket.AvailableDataLength;
                        if (dataLength == 0 || hasPacketPassedLossTest) break;

                        if (random.NextDouble() >= receiveLossProbability)
                        {
                            hasPacketPassedLossTest = true;
                            break;
                        }

                        // Drop packet
                        IPv4EndPoint dummyEndPoint;
                        socket.Receive(dummyBuffer, out dummyEndPoint);
                    }

                    return dataLength;
                }
            }
        }
        #endregion

        #region Methods
        public override void Send(Subarray<byte> data, IPv4EndPoint endPoint)
        {
            if (sendLossProbability > 0)
            {
                lock (@lock)
                {
                    if (random.NextDouble() < sendLossProbability) return;
                }
            }

            socket.Send(data, endPoint);
        }

        public override void Broadcast(Subarray<byte> data, ushort port)
        {
            if (sendLossProbability > 0)
            {
                lock (@lock)
                {
                    if (random.NextDouble() < sendLossProbability) return;
                }
            }

            socket.Broadcast(data, port);
        }

        public override void Receive(Subarray<byte> data, out IPv4EndPoint endPoint)
        {
            if (receiveLossProbability == 0)
            {
                socket.Receive(data, out endPoint);
                return;
            }

            lock (@lock)
            {
                while (true)
                {
                    socket.Receive(data, out endPoint);
                    if (hasPacketPassedLossTest)
                    {
                        hasPacketPassedLossTest = false;
                        break;
                    }

                    if (random.NextDouble() >= receiveLossProbability) break;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) socket.Close();

            base.Dispose(disposing);
        }
        #endregion
    }
}
