using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// Abstract base class for commands, the atomic unit of game state change
    /// which encapsulate an order given by a <see cref="Commander"/>.
    /// </summary>
    public abstract class Command
    {
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
        /// <param name="world">The <see cref="Match"/> providing a context in which to test the handles.</param>
        /// <returns><c>True</c> if all handles of this <see cref="Command"/> are still valid, false otherwise.</returns>
        public abstract bool ValidateHandles(Match match);

        protected bool IsValidFactionHandle(World world, Handle handle)
        {
            return world.FindFactionFromHandle(handle) != null;
        }

        protected bool IsValidFactionHandle(Match match, Handle handle)
        {
            return IsValidFactionHandle(match.World, handle);
        }

        protected bool IsValidEntityHandle(World world, Handle handle)
        {
            return world.Entities.FromHandle(handle) != null;
        }

        protected bool IsValidEntityHandle(Match match, Handle handle)
        {
            return IsValidEntityHandle(match.World, handle);
        }

        protected bool IsValidTechnologyHandle(Match match, Handle handle)
        {
            return match.TechnologyTree.FromHandle(handle) != null;
        }

        protected bool IsValidUnitTypeHandle(Match match, Handle handle)
        {
            return match.UnitTypes.FromHandle(handle) != null;
        }
        #endregion

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> in which the command should be executed.</param>
        public abstract void Execute(Match match);

        public abstract override string ToString();
        #endregion
        #endregion

        #region Static
        #region Fields
        public static readonly BinarySerializer<Command> Serializer;
        #endregion

        #region Constructor
        static Command()
        {
            Serializer = BinarySerializer<Command>.FromCallingAssemblyExportedTypes();
        }
        #endregion

        #region Methods
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
    }
}
