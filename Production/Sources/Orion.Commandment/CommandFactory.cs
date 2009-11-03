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
                Repair.Serializer.Instance

            };

            if (serializers.Select(serializer => serializer.ID).Distinct().Count() != serializers.Count)
                throw new Exception("Two or more CommandSerializers have the same ID.");
        }
        #endregion

        #region Properties
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

            byte commandID = reader.ReadByte();
            CommandSerializer serializer = serializers.FirstOrDefault(s => s.ID == commandID);
            if (serializer == null) throw new InvalidDataException("Unknown command identifier.");
            
            return serializer.Deserialize(reader, world);
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
            CommandSerializer serializer = serializers.FirstOrDefault(s => s.Type == commandType);
            if (serializer == null)
            {
                throw new ArgumentException(
                    "No registered command serializer can handle commands of type {0}."
                    .FormatInvariant(commandType.FullName));
            }

            serializer.Serialize(command, writer);
        }
        #endregion
    }
}
