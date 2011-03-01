using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes a <see cref="Entity"/>
    /// to be assigned the <see cref="BuildTask"/> task.
    /// </summary>
    public sealed class BuildCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> builderHandles;
        private readonly Handle buildingTypeHandle;
        private readonly Point location;
        #endregion

        #region Constructors
        public BuildCommand(Handle factionHandle, IEnumerable<Handle> builderHandles, Handle buildingTypeHandle, Point location)
            : base(factionHandle)
        {
            this.builderHandles = builderHandles.ToList().AsReadOnly();
            this.buildingTypeHandle = buildingTypeHandle;
            this.location = location;
        }

        public BuildCommand(Handle factionHandle, Handle builderHandle, Handle buildingTypeHandle, Point location)
            : this(factionHandle, new[] { builderHandle }, buildingTypeHandle, location) { }
        #endregion

        #region Properties
        public override bool IsMandatory
        {
            get { return false; }
        }

        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return builderHandles; }
        }

        public Handle BuildingTypeHandle
        {
            get { return buildingTypeHandle; }
        }

        public Point Destination
        {
            get { return location; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && builderHandles.All(builderHandle => IsValidEntityHandle(match, builderHandle))
                && IsValidUnitTypeHandle(match, buildingTypeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            Unit buildingType = match.UnitTypes.FromHandle(buildingTypeHandle);

            if (buildingType.Components.Has<AlageneExtractor>())
            {
                Harvestable harvestingInfo = match.World.Entities
                    .Where(e => e.Components.Has<Harvestable>())
                    .Select(e => e.Components.Get<Harvestable>())
                    .Where(h => h.Type == ResourceType.Alagene)
                    .FirstOrDefault(h => !h.IsEmpty);
                Debug.Assert(harvestingInfo != null, "Extractors can only be built on resource node of Alagene.");
            }

            BuildingPlan plan = new BuildingPlan(faction, buildingType, location);

            foreach (Handle unit in builderHandles)
            {
                Entity builder = match.World.Entities.FromHandle(unit);
                builder.Components.Get<TaskQueue>().Enqueue(new BuildTask(builder, plan));
            }
        }

        public override string ToString()
        {
            return "Faction {0} builds {1} with {2} at {3}"
                .FormatInvariant(FactionHandle, buildingTypeHandle,
                builderHandles.ToCommaSeparatedValues(), location);
        }

        #region Serialization
        public static void Serialize(BuildCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.builderHandles);
            WriteHandle(writer, command.buildingTypeHandle);
            writer.Write((short)command.location.X);
            writer.Write((short)command.location.Y);
        }

        public static BuildCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            IEnumerable<Handle> builderHandles = ReadLengthPrefixedHandleArray(reader);
            Handle buildingTypeHandle = ReadHandle(reader);
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            Point location = new Point(x, y);
            return new BuildCommand(factionHandle, builderHandles, buildingTypeHandle, location);
        }
        #endregion
        #endregion
    }
}
