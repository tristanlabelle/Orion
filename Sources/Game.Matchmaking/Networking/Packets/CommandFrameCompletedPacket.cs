using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Engine;
using System.Diagnostics;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet which indicates that a peer has received all commands for a frame.
    /// </summary>
    public sealed class CommandFrameCompletedPacket : GamePacket
    {
        #region Fields
        private readonly int commandFrameNumber;
        private readonly int updateFrameCount;
        private readonly int worldStateHashCode;
        #endregion

        #region Constructors
        public CommandFrameCompletedPacket(int commandFrameNumber, int updateFrameCount,
            int worldStateHashCode)
        {
            Argument.EnsurePositive(commandFrameNumber, "commandFrameNumber");
            Argument.EnsureStrictlyPositive(updateFrameCount, "updateFrameCount");

            this.commandFrameNumber = commandFrameNumber;
            this.updateFrameCount = updateFrameCount;
            this.worldStateHashCode = worldStateHashCode;
        }
        #endregion

        #region Properties
        public int CommandFrameNumber
        {
            get { return commandFrameNumber; }
        }

        public int UpdateFrameCount
        {
            get { return updateFrameCount; }
        }

        public int WorldStateHashCode
        {
            get { return worldStateHashCode; }
        }
        #endregion

        #region Methods
        public static void Serialize(CommandFrameCompletedPacket packet, BinaryWriter writer)
        {
            Argument.EnsureNotNull(packet, "packet");
            Argument.EnsureNotNull(writer, "writer");

            writer.Write(packet.commandFrameNumber);
            writer.Write(packet.updateFrameCount);
            writer.Write(packet.worldStateHashCode);
        }

        public static CommandFrameCompletedPacket Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            int commandFrameNumber = reader.ReadInt32();
            int updateFrameCount = reader.ReadInt32();
            int worldStateHashCode = reader.ReadInt32();
            return new CommandFrameCompletedPacket(commandFrameNumber, updateFrameCount,
                worldStateHashCode);
        }
        #endregion
    }
}
