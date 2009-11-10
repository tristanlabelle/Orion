﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Networking
{
    /// <summary>
    /// Identifies the purpose of a packet.
    /// </summary>
    internal enum PacketType : byte
    {
        /// <summary>
        /// Identifies a packet as containing client data.
        /// </summary>
        Data = 0,

        /// <summary>
        /// Identifies a packet as being an acknowledgement of the reception of a data packet.
        /// </summary>
        Acknowledgement = 1
    }
}