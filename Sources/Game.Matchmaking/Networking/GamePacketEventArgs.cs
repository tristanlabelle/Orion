using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking.Networking.Packets;
using Orion.Engine;

namespace Orion.Game.Matchmaking.Networking
{
    public struct GamePacketEventArgs
    {
        #region Fields
        private readonly IPv4EndPoint senderEndPoint;
        private readonly GamePacket packet;
        #endregion

        #region Constructors
        public GamePacketEventArgs(IPv4EndPoint senderEndPoint, GamePacket packet)
        {
            Argument.EnsureNotNull(packet, "packet");
            this.senderEndPoint = senderEndPoint;
            this.packet = packet;
        }
        #endregion

        #region Properties
        public IPv4EndPoint SenderEndPoint
        {
            get { return senderEndPoint; }
        }

        public GamePacket Packet
        {
            get { return packet; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "{0} from {1}".FormatInvariant(packet.GetType().Name, senderEndPoint);
        }
        #endregion
    }
}
