using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
    internal sealed class SendingTransaction : Transaction
    {
        #region Fields

        internal static byte[] dataByteArray = Encoding.ASCII.GetBytes("DATA");
        internal static byte[] secondAcknowledgeByteArray = Encoding.ASCII.GetBytes("2ACK");
        internal static readonly int dataSignature = BitConverter.ToInt32(dataByteArray, 0);
        internal static readonly int secondAcknowledgeSignature = BitConverter.ToInt32(secondAcknowledgeByteArray, 0);

        private bool successfullySentData;
        private byte[] data;

        #endregion

        #region Constructors

        public SendingTransaction(Transporter transporter, IPEndPoint host, byte[] dataToSend)
            : base(transporter, host)
        {
            data = new byte[dataToSend.Length + 4];
            dataByteArray.CopyTo(data, 0);
            dataToSend.CopyTo(data, dataByteArray.Length);
        }

        #endregion

        #region Properties

        public override bool IsCompleted
        {
            get { return successfullySentData; }
        }

        #endregion

        #region Methods

        public override byte[] Send()
        {
            ResetSendingTimeout();
            packetId++;

            if (!successfullySentData)
                return data;
            return secondAcknowledgeByteArray;
        }

        public override void Receive(byte[] data)
        {
            ResetTransactionTimeout();

            int packetSignature = BitConverter.ToInt32(data, 0);
            if (packetSignature == ReceivingTransaction.firstAcknowledgeSignature)
            {
                successfullySentData = true;
            }
            else 
            {
                throw new ArgumentException("Sending Transaction received packet signature not that of a first acknowledgement");
            }
        }

        #endregion
    }
}
