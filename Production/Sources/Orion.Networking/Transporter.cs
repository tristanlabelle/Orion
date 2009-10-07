using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Orion.Networking
{
    public sealed class Transporter : IDisposable
    {
        #region Fields
        private readonly Socket udpSocket;
        private readonly List<Transaction> transactions;
        private readonly Thread senderThread;
        private readonly Thread receiverThread;
        private bool isDisposed;



        public readonly int Port;
        #endregion

        #region Events

        public event GenericEventHandler<Transporter, NetworkEventArgs> Received;

        #endregion

        #region Constructor

        public Transporter(int port)
        {
            Port = port;
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            transactions = new List<Transaction>();
            senderThread = new Thread(SenderThread);
            receiverThread = new Thread(ReceiverThread);
        }
        #endregion



        #region Methods

        private void SenderThread()
        {
            while (true)
            {
                if (isDisposed)
                    break;
            }
        }

        private void ReceiverThread()
        {
            while (true)
            {
                if (isDisposed)
                    break;
            }
        }

        private void CheckIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(null);
            }
        }


        public void SendTo(byte[] data, IPEndPoint to)
        {
            //to do: create a transaction to send stuff over
        }

        public void Dispose()
        {
            isDisposed = true;
        }


        #endregion
    }

}
