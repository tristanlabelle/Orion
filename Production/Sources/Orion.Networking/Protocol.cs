using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Networking
{
    /// <summary>
    /// Encapsulates the knowledge of the format in which packets are sent.
    /// </summary>
    /// <remarks>
    /// Keeping all formatting in here facilitates future changes to the protocol.
    /// </remarks>
    internal static class Protocol
    {
        #region Fields
        private const byte MagicIdentifier = 0x94;
        #endregion

        #region Methods
        #region Encoding
        public static byte[] CreateDataPacket(byte[] message, uint number)
        {
            Argument.EnsureNotNull(message, "message");
            byte[] packetData = new byte[6 + message.Length];
            packetData[0] = MagicIdentifier;
            packetData[1] = (byte)PacketType.Message;
            BitConverter.GetBytes(number).CopyTo(packetData, 2);
            message.CopyTo(packetData, 6);
            return packetData;
        }

        public static byte[] CreateAcknowledgementPacket(uint number)
        {
            byte[] packetData = new byte[6];
            packetData[0] = MagicIdentifier;
            packetData[1] = (byte)PacketType.Acknowledgement;
            BitConverter.GetBytes(number).CopyTo(packetData, 2);
            return packetData;
        }

        public static byte[] CreateBroadcastPacket(byte[] message)
        {
            Argument.EnsureNotNull(message, "message");
            byte[] packetData = new byte[2 + message.Length];
            packetData[0] = MagicIdentifier;
            packetData[1] = (byte)PacketType.Broadcast;
            message.CopyTo(packetData, 2);
            return packetData;
        }
        #endregion

        #region Decoding
        public static bool IsForeign(byte[] data)
        {
            Argument.EnsureNotNull(data, "data");
            return data.Length == 0 || data[0] != MagicIdentifier;
        }

        public static bool IsValid(byte[] data)
        {
            Argument.EnsureNotNull(data, "data");

            if (data.Length < 2) return false;
            if (data[0] == MagicIdentifier) return false;

            PacketType packetType = (PacketType)data[1];
            if (!Enum.IsDefined(typeof(PacketType), packetType)) return false;
            if (packetType == PacketType.Broadcast) return true;
            return data.Length >= 2 + sizeof(uint);
        }

        public static PacketType GetPacketType(byte[] data)
        {
            Argument.EnsureNotNull(data, "data");
            return (PacketType)data[1];
        }


        public static uint GetDataPacketNumber(byte[] data)
        {
            Argument.EnsureNotNull(data, "data");
            return BitConverter.ToUInt32(data, 2);
        }

        public static uint GetAcknowledgementPacketNumber(byte[] data)
        {
            return GetDataPacketNumber(data);
        }

        public static byte[] GetDataPacketMessage(byte[] data, int dataLength)
        {
            Argument.EnsureNotNull(data, "data");
            byte[] message = new byte[dataLength - 6];
            Array.Copy(data, 6, message, 0, message.Length);
            return message;
        }

        public static byte[] GetBroadcastPacketMessage(byte[] data, int dataLength)
        {
            Argument.EnsureNotNull(data, "data");
            byte[] message = new byte[dataLength- 2];
            Array.Copy(data, 2, message, 0, message.Length);
            return message;
        }
        #endregion
        #endregion
    }
}
