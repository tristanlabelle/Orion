using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes a <see cref="Unit"/>
    /// to be assigned the <see cref="BuildTask"/> task.
    /// </summary>
    public sealed class BuildCommand : Command, IMultipleExecutingEntitiesCommand
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
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
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
        public IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles)
        {
            return new BuildCommand(FactionHandle, entityHandles, buildingTypeHandle, location);
        }

        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && builderHandles.All(builderHandle => IsValidEntityHandle(world, builderHandle))
                && IsValidUnitTypeHandle(world, buildingTypeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            UnitType buildingType = (UnitType)match.World.UnitTypes.FromHandle(buildingTypeHandle);

            if (buildingType.HasSkill<ExtractAlageneSkill>())
            {
                ResourceNode node = match.World.Entities
                    .OfType<ResourceNode>()
                    .FirstOrDefault(n => n.BoundingRectangle.ContainsPoint(location) && n.Type == ResourceType.Alagene);
                Debug.Assert(node != null, "Extractors can only be built on resource node  of Alagene.");
            }

            BuildingPlan plan = new BuildingPlan(faction, buildingType, location);

            foreach (Handle unit in builderHandles)
            {
                Unit builder = (Unit)match.World.Entities.FromHandle(unit);
                builder.TaskQueue.OverrideWith(new BuildTask(builder, plan));
            }
        }

        public override string ToString()
        {
            return "Faction {0} builds {1} with {2} at {3}"
                .FormatInvariant(FactionHandle, buildingTypeHandle,
                builderHandles.ToCommaSeparatedValues(), location);
        }

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, builderHandles);
            WriteHandle(writer, buildingTypeHandle);
            writer.Write((short)location.X);
            writer.Write((short)location.Y);
        }

        public static BuildCommand DeserializeSpecific(BinaryReader reader)
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
