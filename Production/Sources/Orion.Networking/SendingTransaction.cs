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
        private DataHolder dataPacket;
        private DataHolder secondAcknowledgePacket;

        #endregion

        #region Constructors

        public SendingTransaction(Transporter transporter, IPEndPoint host, byte[] dataToSend)
            : base(transporter, host)
        {
            dataPacket = new DataHolder(0, dataToSend);
            secondAcknowledgePacket = new DataHolder(secondAcknowledgeByteArray);
        }

        #endregion

        #region Properties

        public override bool IsCompleted
        {
            get { return successfullySentData; }
        }

        public override byte[] Data
        {
            get { return dataPacket.Data; }
        }

        #endregion

        #region Methods

        public override byte[] Send()
        {
            if (!successfullySentData)
            {
                PrepareSend(dataPacket);
                return dataPacket.Data;
            }

            PrepareSend(secondAcknowledgePacket);
            return secondAcknowledgePacket.Data;
        }

        public override void Receive(byte[] data)
        {
            ResetTransactionTimeout();
            DataHolder holder = new DataHolder(data);

            if (holder.TypeSignature != ReceivingTransaction.firstAcknowledgeSignature)
            {
                throw new ArgumentException("Sending Transaction received packet signature not that of a first acknowledgement");
            }
            successfullySentData = true;
            ReceivedAnswerForPacket(holder.RemotePacketId);
        }

        #endregion
    }
}
