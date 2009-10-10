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

		private bool receivedInitialData;
        private bool receivedReceptionConfirmation;
        private DataHolder firstAcknowledgePacket;
        private DataHolder dataPacket;

        #endregion

        #region Constructors
        public ReceivingTransaction(Transporter transporter, IPEndPoint host)
            : base(transporter, host)
        {
            firstAcknowledgePacket = new DataHolder(firstAcknowledgeByteArray);
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
			
			if(!receivedInitialData)
			{
				if(holder.TypeSignature != SendingTransaction.dataSignature)
				{
					throw new ArgumentException("Receiving Transaction first packet signature not that of a data packet");
				}
				dataPacket = holder;
			}
			else
			{
	            if (holder.TypeSignature != SendingTransaction.secondAcknowledgeSignature)
	            {
	                throw new ArgumentException("Receiving Transaction second packet signature not that of a second acknowledgement packet");
	            }
				receivedReceptionConfirmation = true;
            		firstAcknowledgePacket.RemotePacketId = holder.PacketId;
			}

            ResetTransactionTimeout();

        }
        public override byte[] Send()
        {
            PrepareSend(firstAcknowledgePacket);
            return firstAcknowledgePacket.Bytes;
        }

        #endregion
    }
}
