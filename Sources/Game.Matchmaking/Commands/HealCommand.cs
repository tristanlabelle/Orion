using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Matchmaking.Commands
{
    public sealed class HealCommand : Command, IMultipleExecutingEntitiesCommand
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        private readonly Handle targetHandle;
        #endregion

        #region Constructors
        public HealCommand(Handle factionHandle, IEnumerable<Handle> units, Handle targetHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(units, "units");
            if (units.Contains(targetHandle))
                throw new ArgumentException("A unit cannot Heal itself.");

            this.unitHandles = units.Distinct().ToList().AsReadOnly();
            this.targetHandle = targetHandle;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return unitHandles; }
        }

        public Handle TargetHandle
        {
            get { return targetHandle; }
        }
        #endregion

        #region Methods
        public IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles)
        {
            return new HealCommand(FactionHandle, entityHandles, targetHandle);
        }

        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && unitHandles.All(handle => IsValidEntityHandle(match, handle))
                && IsValidEntityHandle(match, targetHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Unit target = (Unit)match.World.Entities.FromHandle(targetHandle);
            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)match.World.Entities.FromHandle(unitHandle);
                unit.TaskQueue.OverrideWith(new HealTask(unit, target));
            }
        }

        public override string ToString()
        {
            return "Faction {0} repairs {1} with {2}"
                .FormatInvariant(FactionHandle, targetHandle, unitHandles.ToCommaSeparatedValues());
        }
                        
        #region Serialization
        public static void Serialize(HealCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.unitHandles);
            WriteHandle(writer, command.targetHandle);
        }

        public static HealCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            Handle targetHandle = ReadHandle(reader);
            return new HealCommand(factionHandle, unitHandles, targetHandle);
        }
        #endregion
        #endregion
    }
}