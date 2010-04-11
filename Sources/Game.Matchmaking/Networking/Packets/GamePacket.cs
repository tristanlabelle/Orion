using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// Base class for packets of data exchanged between game clients.
    /// </summary>
    public abstract class GamePacket
    {
        #region Static
        #region Fields
        public static readonly BinarySerializer<GamePacket> Serializer;
        #endregion

        #region Constructor
        static GamePacket()
        {
            Serializer = BinarySerializer<GamePacket>.FromCallingAssemblyExportedTypes();
        }
        #endregion
        #endregion
    }
}
