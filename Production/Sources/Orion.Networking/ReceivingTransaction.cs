using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
    internal abstract class ReceivingTransaction : Transaction
    {
        #region Nested Types

        private enum TransationState
        {
            Ready, ReceivedData, ReceivedSecondAcknowledgement, Done
        }

        #endregion

        #region Fields

        internal static byte[] firstAcknowledgeByteArray = Encoding.ASCII.GetBytes("1ACK");
        internal static byte[] completedByteArray = Encoding.ASCII.GetBytes("THX");
        internal static readonly int firstAcknowledgeSignature = BitConverter.ToInt32(firstAcknowledgeByteArray, 0);
        internal static readonly int completedSignature = BitConverter.ToInt32(completedByteArray, 0);

        #endregion

        public ReceivingTransaction(Transporter transporter, IPEndPoint host)
            : base(transporter, host)
        { }
    }
}
