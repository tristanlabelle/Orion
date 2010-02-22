using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Networking
{
    /// <summary>
    /// Identifies the purpose of a packet.
    /// </summary>
    internal enum PacketType : byte
    {
        /// <summary>
        /// Identifies a packet as containing client data.
        /// </summary>
        Message = 202,

        /// <summary>
        /// Identifies a packet as being an acknowledgement of the reception of a data packet.
        /// </summary>
        Acknowledgement = 203,

        /// <summary>
        /// Identifies a packet as being broadcasted, and thus does not implies an acknowledgement.
        /// </summary>
        Broadcast = 204,

        /// <summary>
        /// Identifies a ping packet that should just be ignored.
        /// </summary>
        Ping = 205
    }
}
