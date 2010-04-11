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
        #endregion

        #region Constructors
        public CommandFrameCompletedPacket(int commandFrameNumber, int updateFrameCount)
        {
            Argument.EnsurePositive(commandFrameNumber, "commandFrameNumber");
            Argument.EnsureStrictlyPositive(updateFrameCount, "updateFrameCount");

            this.commandFrameNumber = commandFrameNumber;
            this.updateFrameCount = updateFrameCount;
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
        #endregion

        #region Methods
        public static void Serialize(CommandFrameCompletedPacket packet, BinaryWriter writer)
        {
            Argument.EnsureNotNull(packet, "packet");
            Argument.EnsureNotNull(writer, "writer");

            writer.Write(packet.commandFrameNumber);
            writer.Write(packet.updateFrameCount);
        }

        public static CommandFrameCompletedPacket Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            int commandFrameNumber = reader.ReadInt32();
            int updateFrameCount = reader.ReadInt32();
            return new CommandFrameCompletedPacket(commandFrameNumber, updateFrameCount);
        }
        #endregion
    }
}
