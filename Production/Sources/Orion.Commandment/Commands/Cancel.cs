using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Orion.GameLogic;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which cancels the current <see cref="Task"/> of a set of <see cref="Unit"/>s.
    /// </summary>
    [Serializable]
    public sealed class Cancel : Command
    {
        #region Instance
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        #endregion

        #region Constructors
        public Cancel(Handle factionHandle, IEnumerable<Handle> unitHandles)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(unitHandles, "unitHandles");
            this.unitHandles = unitHandles.Distinct().ToList().AsReadOnly();
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return unitHandles; }
        }
        #endregion

        #region Methods
        public override void Execute(World world)
        {
            Argument.EnsureNotNull(world, "world");
            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)world.Entities.FindFromHandle(unitHandle);
                unit.Task = null;
            }
        }

        public override string ToString()
        {
            return "[{0}] cancel".FormatInvariant(unitHandles.ToCommaSeparatedValues());
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Cancel"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<Cancel>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Cancel command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteLengthPrefixedHandleArray(writer, command.unitHandles);
            }

            protected override Cancel DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                var unitHandles = ReadLengthPrefixedHandleArray(reader);
                return new Cancel(factionHandle, unitHandles);
            }
            #endregion
            #endregion

            #region Static
            #region Fields
            /// <summary>
            /// A globally available static instance of this class.
            /// </summary>
            public static readonly Serializer Instance = new Serializer();
            #endregion
            #endregion
        }
        #endregion
    }
}
