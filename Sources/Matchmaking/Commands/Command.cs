using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Orion.Collections;
using Orion.GameLogic;

namespace Orion.Matchmaking.Commands
{
    /// <summary>
    /// Abstract base class for commands, the atomic unit of game state change
    /// which encapsulate an order given by a <see cref="Commander"/>.
    /// </summary>
    public abstract class Command
    {
        #region Nested Types
        private struct CommandType
        {
            public readonly Type Type;
            public readonly byte Code;
            public readonly Func<BinaryReader, Command> Deserializer;

            public CommandType(Type type, byte code, Func<BinaryReader, Command> deserializer)
            {
                this.Type = type;
                this.Code = code;
                this.Deserializer = deserializer;
            }
        }
        #endregion

        #region Instance
        #region Fields
        private readonly Handle factionHandle;
        #endregion

        #region Constructors
        protected internal Command(Handle factionHandle)
        {
            this.factionHandle = factionHandle;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the handle of the faction which created this command.
        /// </summary>
        public Handle FactionHandle
        {
            get { return factionHandle; }
        }

        /// <summary>
        /// Gets a sequence of handles to <see cref="Entity">entities</see> executing in this command.
        /// </summary>
        public abstract IEnumerable<Handle> ExecutingEntityHandles { get; }
        #endregion

        #region Methods
        #region Validation
        /// <summary>
        /// Checks if the handles referenced in this <see cref="Command"/> are still valid.
        /// </summary>
        /// <param name="world">The <see cref="World"/> providing a context in which to test the handles.</param>
        /// <returns><c>True</c> if all handles of this <see cref="Command"/> are still valid, false otherwise.</returns>
        public abstract bool ValidateHandles(World world);

        protected bool IsValidFactionHandle(World world, Handle handle)
        {
            return world.FindFactionFromHandle(handle) != null;
        }

        protected bool IsValidEntityHandle(World world, Handle handle)
        {
            return world.Entities.FromHandle(handle) != null;
        }

        protected bool IsValidTechnologyHandle(World world, Handle handle)
        {
            return world.TechnologyTree.FromHandle(handle) != null;
        }

        protected bool IsValidUnitTypeHandle(World world, Handle handle)
        {
            return world.UnitTypes.FromHandle(handle) != null;
        }
        #endregion

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> in which the command should be executed.</param>
        public abstract void Execute(Match match);

        public abstract override string ToString();

        #region Serialization
        /// <summary>
        /// Serializes this <see cref="Command"/> to a stream.
        /// </summary>
        /// <param name="writer">A <see cref="BinaryWriter"/> to be used to write serialized data.</param>
        public void Serialize(BinaryWriter writer)
        {
            Argument.EnsureNotNull(writer, "writer");

            Type type = GetType();
            byte code = (byte)commandTypes.IndexOf(t => t.Type == type);
            writer.Write(code);
            SerializeSpecific(writer);
        }

        /// <summary>
        /// Serializes command type-specific information to a stream.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> where to write.</param>
        protected abstract void SerializeSpecific(BinaryWriter writer);

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
        #endregion

        #region Static
        #region Fields
        private static readonly List<CommandType> commandTypes;
        #endregion

        #region Constructor
        static Command()
        {
            commandTypes = new List<CommandType>();

            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => typeof(Command).IsAssignableFrom(type) && !type.IsAbstract)
                .OrderBy(type => type.FullName);

            var argumentTypes = new[] { typeof(BinaryReader) };
            foreach (Type type in types)
            {
                Debug.Assert(type.IsPublic);
                MethodInfo method = type.GetMethod("DeserializeSpecific",
                    BindingFlags.Static | BindingFlags.Public, Type.DefaultBinder,
                    argumentTypes, null);
                Debug.Assert(method != null);
                Debug.Assert(typeof(Command).IsAssignableFrom(method.ReturnType));
                var deserializer = (Func<BinaryReader, Command>)
                    Delegate.CreateDelegate(typeof(Func<BinaryReader, Command>), method);
                commandTypes.Add(new CommandType(type, (byte)commandTypes.Count, deserializer));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Deserializes a command from a stream of data.
        /// </summary>
        /// <param name="reader">A <see cref="BinaryReader"/> over a stream of data.</param>
        /// <returns>The command that was deserialized.</returns>
        public static Command Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            byte code = reader.ReadByte();
            if (code >= commandTypes.Count) throw new InvalidDataException("Invalid command type code.");
            return commandTypes[code].Deserializer(reader);
        }
        #endregion
        #endregion
    }
}
