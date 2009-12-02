using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using RepairTask = Orion.GameLogic.Tasks.Repair;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Unit"/>s
    /// to attack another <see cref="Unit"/>.
    /// </summary>
    public sealed class Repair : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        private readonly Handle targetHandle;
        #endregion

        #region Constructors
        public Repair(Handle factionHandle, IEnumerable<Handle> units, Handle targetHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(units, "units");
            if (units.Contains(targetHandle))
                throw new ArgumentException("A unit cannot repair itself.");

            this.unitHandles = units.Distinct().ToList().AsReadOnly();
            this.targetHandle = targetHandle;
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
                && unitHandles.All(handle => IsValidEntityHandle(world, handle))
                && IsValidEntityHandle(world, targetHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Unit target = (Unit)match.World.Entities.FromHandle(targetHandle);
            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)match.World.Entities.FromHandle(unitHandle);
                unit.CurrentTask = new RepairTask(unit, target);
            }
        }

        public override string ToString()
        {
            return "Faction {0} repairs {1} with {2}"
                .FormatInvariant(FactionHandle, targetHandle, unitHandles.ToCommaSeparatedValues());
        }
                        
        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, unitHandles);
            WriteHandle(writer, targetHandle);
        }

        public static Repair DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            Handle targetHandle = ReadHandle(reader);
            return new Repair(factionHandle, unitHandles, targetHandle);
        }
        #endregion
        #endregion
    }
}