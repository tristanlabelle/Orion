using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using System.IO;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A command which causes a unit to upgrade to another unit type.
    /// </summary>
    public sealed class UpgradeCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        private readonly Handle targetUnitTypeHandle;
        #endregion

        #region Constructors
        public UpgradeCommand(Handle factionHandle, IEnumerable<Handle> unitHandles, Handle targetUnitTypeHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(unitHandles, "trainerHandles");

            this.unitHandles = unitHandles.Distinct().ToList().AsReadOnly();
            this.targetUnitTypeHandle = targetUnitTypeHandle;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return unitHandles; }
        }

        public Handle TargetUnitTypeHandle
        {
            get { return targetUnitTypeHandle; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && unitHandles.All(handle => IsValidEntityHandle(match, handle))
                && IsValidUnitTypeHandle(match, targetUnitTypeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            UnitType targetUnitType = match.UnitTypes.FromHandle(targetUnitTypeHandle);
            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)match.World.Entities.FromHandle(unitHandle);
                unit.Type = targetUnitType;
            }
        }

        public override string ToString()
        {
            return "Faction {0} upgrades {1} to {2}"
                .FormatInvariant(FactionHandle, unitHandles.ToCommaSeparatedValues(), targetUnitTypeHandle);
        }
        
        #region Serialization
        public static void Serialize(UpgradeCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.unitHandles);
            WriteHandle(writer, command.targetUnitTypeHandle);
        }

        public static UpgradeCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            Handle targetUnitTypeHandle = ReadHandle(reader);
            return new UpgradeCommand(factionHandle, unitHandles, targetUnitTypeHandle);
        }
        #endregion
        #endregion
    }
}
