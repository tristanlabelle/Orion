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
    /// A command which causes an <see cref="Entity"/> to upgrade to another prototype.
    /// </summary>
    public sealed class UpgradeCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        private readonly Handle targetPrototypeHandle;
        #endregion

        #region Constructors
        public UpgradeCommand(Handle factionHandle, IEnumerable<Handle> entityHandles, Handle targetPrototypeHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(entityHandles, "entityHandles");

            this.unitHandles = entityHandles.Distinct().ToList().AsReadOnly();
            this.targetPrototypeHandle = targetPrototypeHandle;
        }

        public UpgradeCommand(Handle factionHandle, Handle entityHandles, Handle targetPrototypeHandle)
            : this(factionHandle, new[] { entityHandles }, targetPrototypeHandle) { }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return unitHandles; }
        }

        public Handle TargetUnitTypeHandle
        {
            get { return targetPrototypeHandle; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && unitHandles.All(handle => IsValidEntityHandle(match, handle))
                && IsValidUnitTypeHandle(match, targetPrototypeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);

            Entity targetPrototype = match.Prototypes.FromHandle(targetPrototypeHandle);
            int targetUnitTypeFoodCost = (int)faction.GetStat(targetPrototype, Cost.FoodStat);

            foreach (Handle unitHandle in unitHandles)
            {
                Entity entity = match.World.Entities.FromHandle(unitHandle);
                EntityUpgrade upgrade = entity.Identity.Upgrades.FirstOrDefault(u => u.Target == targetPrototype.Identity.Name);
                if (upgrade == null)
                {
                    throw new InvalidOperationException(
                        "Unit {0} cannot upgrade to {1}."
                        .FormatInvariant(entity.Identity.Name, upgrade.Target));
                }

                int unitFoodCost = (int)entity.GetStatValue(Cost.FoodStat);

                if (upgrade.AladdiumCost > faction.AladdiumAmount || upgrade.AlageneCost > faction.AlageneAmount)
                {
                    faction.RaiseWarning("Pas assez de ressources pour upgrader à {0}.".FormatInvariant(targetPrototype.Identity.Name));
                    return;
                }

                if (targetUnitTypeFoodCost > unitFoodCost
                    && targetUnitTypeFoodCost - unitFoodCost > faction.RemainingFoodAmount)
                {
                    faction.RaiseWarning("Pas assez de nourriture pour upgrader à {0}.".FormatInvariant(targetPrototype.Identity.Name));
                    return;
                }

                faction.AladdiumAmount -= upgrade.AladdiumCost;
                faction.AlageneAmount -= upgrade.AlageneCost;
                entity.Identity.UpgradeTo(targetPrototype.Identity);
            }
        }

        public override string ToString()
        {
            return "Faction {0} upgrades {1} to {2}"
                .FormatInvariant(FactionHandle, unitHandles.ToCommaSeparatedValues(), targetPrototypeHandle);
        }
        
        #region Serialization
        public static void Serialize(UpgradeCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.unitHandles);
            WriteHandle(writer, command.targetPrototypeHandle);
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
