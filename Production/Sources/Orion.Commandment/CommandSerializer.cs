using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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

        /// <summary>
        /// Gets the id associated with this <see cref="CommandType"/>.
        /// </summary>
        public abstract byte ID { get; }
        #endregion

        #region Methods
        #region Serialization
        /// <summary>
        /// Serializes a <see cref="Command"/> to a data stream.
        /// </summary>
        /// <param name="command">The <see cref="Command"/> to be serialized.</param>
        /// <param name="writer">A binary writer to be used to write serialized data.</param>
        public abstract void Serialize(Command command, BinaryWriter writer);
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
        public abstract Command Deserialize(BinaryReader reader, World world);

        protected Faction ReadFaction(BinaryReader reader, World world)
        {
            int factionID = reader.ReadInt32();
            Faction faction = world.FindFactionWithID(factionID);
            if (faction == null) throw new InvalidDataException("Invalid faction identifier.");
            return faction;
        }

        protected Unit ReadUnit(BinaryReader reader, World world)
        {
            int unitID = reader.ReadInt32();
            Unit unit = world.Units.FindFromID(unitID);
            if (unit == null) throw new InvalidDataException("Invalid unit identifier.");
            return unit;
        }
        protected UnitType ReadUnitType(BinaryReader reader, World world)
        {
            int unitTypeID = reader.ReadInt32();
            UnitType unitType = world.UnitTypes.FromID(unitTypeID);
            if (unitType == null) throw new InvalidDataException("Invalid unitType identifier.");
            return unitType;
        }

        protected Unit[] ReadLengthPrefixedUnitArray(BinaryReader reader, World world)
        {
            int unitCount = reader.ReadInt32();
            if (unitCount <= 0)
            {
                throw new InvalidDataException(
                    "Invalid number of units: {0}.".FormatInvariant(unitCount));
            }

            Unit[] units = new Unit[unitCount];
            for (int i = 0; i < unitCount; ++i)
                units[i] = ReadUnit(reader, world);

            return units;
        }
        #endregion

        #region Object Model
        public sealed override string ToString()
        {
            return "{0} (#{1})".FormatInvariant(Type.FullName, ID);
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
        public sealed override void Serialize(Command command, BinaryWriter writer)
        {
            Argument.EnsureBaseType(command, Type, "command");
            Argument.EnsureNotNull(writer, "writer");

            writer.Write(ID);
            SerializeData((TCommand)command, writer);
        }

        public sealed override Command Deserialize(BinaryReader reader, World world)
        {
            Argument.EnsureNotNull(reader, "reader");
            Argument.EnsureNotNull(world, "world");

            return DeserializeData(reader, world);
        }

        protected abstract void SerializeData(TCommand command, BinaryWriter writer);
        protected abstract TCommand DeserializeData(BinaryReader reader, World world);
        #endregion
    }
}
