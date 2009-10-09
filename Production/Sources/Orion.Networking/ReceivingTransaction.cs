using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
    internal class ReceivingTransaction : Transaction
    {
        #region Fields
        
        internal static byte[] firstAcknowledgeByteArray = Encoding.ASCII.GetBytes("1ACK");
        internal static readonly int firstAcknowledgeSignature = BitConverter.ToInt32(firstAcknowledgeByteArray, 0);

        private bool receivedReceptionConfirmation;
        private DataHolder firstAcknowledgePacket;
        private DataHolder dataPacket;

        #endregion

        #region Constructors
        public ReceivingTransaction(Transporter transporter, IPEndPoint host, byte[] receivedData)
            : base(transporter, host)
        {
            dataPacket = new DataHolder(receivedData);
            firstAcknowledgePacket = new DataHolder(firstAcknowledgeByteArray);
            if(dataPacket.TypeSignature != SendingTransaction.dataSignature)
            {
                throw new ArgumentException("Receiving Transaction first packet signature not that of a data packet");
            }
        }
        #endregion

        #region Properties

        public override bool IsCompleted 
        { 
            get { return receivedReceptionConfirmation; } 
        }

        public override byte[] Data
        {
            get
            {
                if (!receivedReceptionConfirmation) return null;
                return dataPacket.Data;
            }
        }

        #endregion

        #region Methods

        public override void Receive(byte[] data)
        {
            DataHolder holder = new DataHolder(data);
            if (holder.TypeSignature != SendingTransaction.secondAcknowledgeSignature)
            {
                throw new ArgumentException("Receiving Transaction second packet signature not that of a second acknowledgement packet");
            }

            ResetTransactionTimeout();
            receivedReceptionConfirmation = true;
            firstAcknowledgePacket.RemotePacketId = holder.PacketId;

        }
        public override byte[] Send()
        {
            PrepareSend(firstAcknowledgePacket);
            return firstAcknowledgePacket.Bytes;
        }

        #endregion
    }
}
