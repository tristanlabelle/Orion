using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using RepairTask = Orion.Game.Simulation.Tasks.RepairTask;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Entity"/>s
    /// to attack another <see cref="Entity"/>.
    /// </summary>
    public sealed class RepairCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        private readonly Handle targetHandle;
        #endregion

        #region Constructors
        public RepairCommand(Handle factionHandle, IEnumerable<Handle> units, Handle targetHandle)
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
        public override bool IsMandatory
        {
            get { return false; }
        }

        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return unitHandles; }
        }

        public Handle TargetHandle
        {
            get { return targetHandle; }
        }
        #endregion

        #region Methods
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

            Entity target = match.World.Entities.FromHandle(targetHandle);
            foreach (Handle unitHandle in unitHandles)
            {
                Entity entity = match.World.Entities.FromHandle(unitHandle);
                entity.Components.Get<TaskQueue>().Enqueue(new RepairTask(entity, target));
            }
        }

        public override string ToString()
        {
            return "Faction {0} repairs {1} with {2}"
                .FormatInvariant(FactionHandle, targetHandle, unitHandles.ToCommaSeparatedValues());
        }
                        
        #region Serialization
        public static void Serialize(RepairCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.unitHandles);
            WriteHandle(writer, command.targetHandle);
        }

        public static RepairCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            Handle targetHandle = ReadHandle(reader);
            return new RepairCommand(factionHandle, unitHandles, targetHandle);
        }
        #endregion
        #endregion
    }
}