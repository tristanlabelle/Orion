using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

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

        public UpgradeCommand(Handle factionHandle, Handle unitHandle, Handle targetUnitTypeHandle)
            : this(factionHandle, new[] { unitHandle }, targetUnitTypeHandle) { }
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

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);

            Unit targetUnitType = match.UnitTypes.FromHandle(targetUnitTypeHandle);
            int targetUnitTypeFoodCost = (int)faction.GetStat(targetUnitType, FactionMembership.FoodCostStat);

            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)match.World.Entities.FromHandle(unitHandle);
                UnitTypeUpgrade upgrade = unit.Identity.Upgrades.FirstOrDefault(u => u.Target == targetUnitType.Identity.Name);
                if (upgrade == null)
                {
                    throw new InvalidOperationException(
                        "Unit {0} cannot upgrade to {1}."
                        .FormatInvariant(unit.Identity.Name, upgrade.Target));
                }

                int unitFoodCost = (int)unit.GetStatValue(FactionMembership.FoodCostStat);

                if (upgrade.AladdiumCost > faction.AladdiumAmount || upgrade.AlageneCost > faction.AlageneAmount)
                {
                    faction.RaiseWarning("Pas assez de ressources pour upgrader à {0}.".FormatInvariant(targetUnitType.Identity.Name));
                    return;
                }

                if (targetUnitTypeFoodCost > unitFoodCost
                    && targetUnitTypeFoodCost - unitFoodCost > faction.RemainingFoodAmount)
                {
                    faction.RaiseWarning("Pas assez de nourriture pour upgrader à {0}.".FormatInvariant(targetUnitType.Identity.Name));
                    return;
                }

                faction.AladdiumAmount -= upgrade.AladdiumCost;
                faction.AlageneAmount -= upgrade.AlageneCost;
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
