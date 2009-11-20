using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orion.GameLogic;

namespace Orion.Commandment
{
    /// <summary>
    /// Holds type information for a specific type of <see cref="Command"/>s.
    /// </summary>
    [Serializable]
    public abstract class CommandSerializer
    {
        #region Constructors
        internal CommandSerializer() { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of the <see cref="Command"/>-derived class
        /// represented by this type.
        /// </summary>
        public abstract Type Type { get; }
        #endregion

        #region Methods
        #region Serialization
        /// <summary>
        /// Serializes a <see cref="Command"/> to a data stream.
        /// </summary>
        /// <param name="command">The <see cref="Command"/> to be serialized.</param>
        /// <param name="writer">A binary writer to be used to write serialized data.</param>
        internal abstract void Serialize(Command command, BinaryWriter writer);

        protected static void WriteHandle(BinaryWriter writer, Handle handle)
        {
            writer.Write(handle.Value);
        }

        protected static void WriteLengthPrefixedHandleArray(BinaryWriter writer, IEnumerable<Handle> handles)
        {
            writer.Write(handles.Count());
            foreach (Handle handle in handles)
                WriteHandle(writer, handle);
        }
        #endregion

        #region Deserialization
        /// <summary>
        /// Deserializes a <see cref="Command"/> from a data stream.
        /// </summary>
        /// <param name="reader">A binary reader from which to read serialized data.</param>
        /// <param name="world">
        /// The <see cref="World"/> that is the context of the deserialized <see cref="Command"/>.
        /// </param>
        /// <returns>The <see cref="Command"/> that was deserialized.</returns>
        internal abstract Command Deserialize(BinaryReader reader);

        protected static Handle ReadHandle(BinaryReader reader)
        {
            uint handleValue = reader.ReadUInt32();
            return new Handle(handleValue);
        }

        protected static Handle[] ReadLengthPrefixedHandleArray(BinaryReader reader)
        {
            int unitCount = reader.ReadInt32();
            if (unitCount <= 0)
            {
                throw new InvalidDataException(
                    "Invalid number of units: {0}.".FormatInvariant(unitCount));
            }

            Handle[] units = new Handle[unitCount];
            for (int i = 0; i < unitCount; ++i)
                units[i] = ReadHandle(reader);

            return units;
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// Base class for strongly-typed <see cref="CommandSerializer"/>s.
    /// </summary>
    /// <typeparam name="TCommand">
    /// The type of <see cref="Command"/> this serializer works with.
    /// </typeparam>
    [Serializable]
    public abstract class CommandSerializer<TCommand> : CommandSerializer where TCommand : Command
    {
        #region Properties
        public override sealed Type Type
        {
            get { return typeof(TCommand); }
        }
        #endregion

        #region Methods
        internal sealed override void Serialize(Command command, BinaryWriter writer)
        {
            Argument.EnsureBaseType(command, Type, "command");
            Argument.EnsureNotNull(writer, "writer");

            SerializeData((TCommand)command, writer);
        }

        internal sealed override Command Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");
            return DeserializeData(reader);
        }

        protected abstract void SerializeData(TCommand command, BinaryWriter writer);
        protected abstract TCommand DeserializeData(BinaryReader reader);
        #endregion
    }
}
