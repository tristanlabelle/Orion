using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Orion.Game.Matchmaking.Commands;
using Orion.Engine;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet containing the commands for a command frame.
    /// </summary>
    public sealed class CommandsPacket : GamePacket
    {
        #region Fields
        private readonly ReadOnlyCollection<Command> commands;
        #endregion

        #region Constructors
        public CommandsPacket(IEnumerable<Command> commands)
        {
            Argument.EnsureNotNull(commands, "commands");

            this.commands = commands.ToList().AsReadOnly();
        }
        #endregion

        #region Properties
        public ReadOnlyCollection<Command> Commands
        {
            get { return commands; }
        }
        #endregion

        #region Methods
        public static void Serialize(CommandsPacket packet, BinaryWriter writer)
        {
            Argument.EnsureNotNull(packet, "packet");
            Argument.EnsureNotNull(writer, "writer");

            writer.Write(packet.commands.Count);
            foreach (Command command in packet.commands)
                Command.Serializer.Serialize(command, writer);
        }

        public static CommandsPacket Deserialize(BinaryReader reader)
        {
            int commandCount = reader.ReadInt32();
            var commands = Enumerable.Range(0, commandCount)
                .Select(i => Command.Serializer.Deserialize(reader));
            return new CommandsPacket(commands);
        }
        #endregion
    }
}
