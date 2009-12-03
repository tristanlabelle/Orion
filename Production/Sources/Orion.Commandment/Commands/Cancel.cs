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
        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && unitHandles.All(handle => IsValidEntityHandle(world, handle));
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");
            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)match.World.Entities.FromHandle(unitHandle);
                unit.TaskQueue.Clear();
            }
        }

        public override string ToString()
        {
            return "Faction {0} cancels {1}"
                .FormatInvariant(FactionHandle, unitHandles.ToCommaSeparatedValues());
        }

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, unitHandles);
        }

        public static Cancel DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            return new Cancel(factionHandle, unitHandles);
        }
        #endregion
        #endregion
    }
}
