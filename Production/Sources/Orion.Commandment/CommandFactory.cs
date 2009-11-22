using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orion.Commandment.Commands;
using Orion.GameLogic;

namespace Orion.Commandment
{
    /// <summary>
    /// Provides methods to create <see cref="Command"/>s in a world.
    /// </summary>
    public sealed class CommandFactory
    {
        #region Fields
        private readonly World world;
        private readonly List<CommandSerializer> serializers;
        #endregion

        #region Constructors
        public CommandFactory(World world)
        {
            Argument.EnsureNotNull(world, "world");
            this.world = world;

            serializers = new List<CommandSerializer>()
            {
                Move.Serializer.Instance,
                Attack.Serializer.Instance,
                Cancel.Serializer.Instance,
                ZoneAttack.Serializer.Instance,
                Build.Serializer.Instance,
                Train.Serializer.Instance,
                Harvest.Serializer.Instance,
                Repair.Serializer.Instance,
                Suicide.Serializer.Instance,
                ChangeDiplomacy.Serializer.Instance,
                SendRessources.Serializer.Instance,
                Message.Serializer.Instance
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Deserializes a <see cref="Command"/> from a data stream.
        /// </summary>
        /// <param name="reader">A data stream reader to be used to read the serialized data.</param>
        /// <returns>The <see cref="Command"/> that was deserialized.</returns>
        public Command Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            byte serializerIndex = reader.ReadByte();
            if (serializerIndex >= serializers.Count) throw new InvalidDataException("Unknown command type identifier.");
            CommandSerializer serializer = serializers[serializerIndex];
            
            return serializer.Deserialize(reader);
        }

        /// <summary>
        /// Serializes a <see cref="Command"/> to a data stream.
        /// </summary>
        /// <param name="command">The <see cref="Command"/> to be serialized.</param>
        /// <param name="writer">A data stream writer to be used to write the serialized data.</param>
        public void Serialize(Command command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            Type commandType = command.GetType();
            int serializerIndex = serializers.FindIndex(s => s.Type == commandType);
            if (serializerIndex < 0)
            {
                throw new ArgumentException(
                    "No registered command serializer can handle commands of type {0}."
                    .FormatInvariant(commandType.FullName));
            }

            CommandSerializer serializer = serializers[serializerIndex];
            writer.Write((byte)serializerIndex);
            serializer.Serialize(command, writer);
        }
        #endregion
    }
}
