using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
    internal abstract class ReceivingTransaction : Transaction
    {
        #region Fields
        
        internal static byte[] firstAcknowledgeByteArray = Encoding.ASCII.GetBytes("1ACK");
        internal static readonly int firstAcknowledgeSignature = BitConverter.ToInt32(firstAcknowledgeByteArray, 0);

        private bool receivedReceptionConfirmation;
        private byte[] data;

        #endregion

        #region constructor
        public ReceivingTransaction(Transporter transporter, IPEndPoint host, byte[] data)
            : base(transporter, host)
        { 
            int packetSignature = BitConverter.ToInt32(data,0);
            if(packetSignature != SendingTransaction.dataSignature)
            {
                throw new ArgumentException("Receiving Transaction first packet signature not that of a data packet");
            }
            else
            {
                this.data = data;
            }

        }
        #endregion

        #region Properties

        public override bool IsCompleted 
        { 
            get{return receivedReceptionConfirmation;} 
        }

        #endregion

        #region methods

        public override void Receive(byte[] data)
        {
            ResetTransactionTimeout();
            int packetSignature = BitConverter.ToInt32(data, 0);
            if (packetSignature == SendingTransaction.secondAcknowledgeSignature)
            {
                receivedReceptionConfirmation = true;
            }
            else
            {
                throw new ArgumentException("Receiving Transaction second packet signature not that of a second acknowledgement packet");
            }

        }
        public override byte[] Send()
        {
            ResetSendingTimeout();
            packetId++;

            return firstAcknowledgeByteArray;
        }


        #endregion
    }
}
