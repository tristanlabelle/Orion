using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
	/// <summary>
	/// A ReceivingTransaction is the receiving endpoint of a safe UDP transaction. 
	/// </summary>
    internal class ReceivingTransaction : Transaction
    {
        #region Fields
        
		/// <summary>
		/// Holds the byte array representing the signature of a first acknowledge packet.
		/// </summary>
        internal static byte[] firstAcknowledgeByteArray = Encoding.ASCII.GetBytes("1ACK");
		
		/// <summary>
		/// Holds the signature of a first acknowledge packet. 
		/// </summary>
        internal static readonly int firstAcknowledgeSignature = BitConverter.ToInt32(firstAcknowledgeByteArray, 0);

		private bool receivedInitialData;
        private bool receivedReceptionConfirmation;
        private DataHolder firstAcknowledgePacket;
        private DataHolder dataPacket;

        #endregion

        #region Constructors
		/// <summary>
		/// Constructs a ReceivingTransaction.
		/// </summary>
		/// <param name="transporter">
		/// The <see cref="Transporter"/> that created this transaction
		/// </param>
		/// <param name="host">
		/// The remote host connection's <see cref="IPEndPoint"/>
		/// </param>
        public ReceivingTransaction(Transporter transporter, IPEndPoint host)
            : base(transporter, host)
        {
            firstAcknowledgePacket = new DataHolder(0, firstAcknowledgeByteArray);
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
				receivedInitialData = true;
				dataPacket = holder;
            		firstAcknowledgePacket.RemotePacketId = dataPacket.PacketId;
			}
			else
			{
	            if (holder.TypeSignature != SendingTransaction.secondAcknowledgeSignature)
	            {
	                throw new ArgumentException("Receiving Transaction second packet signature not that of a second acknowledgement packet");
	            }
				receivedReceptionConfirmation = true;
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
