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
        private bool shouldSendSecondAcknowledge;
        private byte[] data;

        #endregion

        #region Constructors

        public SendingTransaction(Transporter transporter, IPEndPoint host, byte[] dataToSend)
            : base(transporter, host)
        {
            data = dataToSend;
        }

        #endregion

        #region Properties

        public override bool IsReady
        {
            get { return !IsCompleted && DateTime.UtcNow >= ResendTimeout; }
        }

        public override bool IsCompleted
        {
            get { return successfullySentData && !shouldSendSecondAcknowledge; }
        }

        #endregion

        #region Methods

        public override byte[] Send()
        {
            ResetSendingTimeout();

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
                shouldSendSecondAcknowledge = true;
            }
            else if (packetSignature == ReceivingTransaction.completedSignature)
            {
                shouldSendSecondAcknowledge = false;
            }
        }

        #endregion
    }
}
