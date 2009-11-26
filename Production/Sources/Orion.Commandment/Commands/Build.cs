using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Math;
using Orion.GameLogic;
using BuildTask = Orion.GameLogic.Tasks.Build;
using System.Linq;
using System.Collections.ObjectModel;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes a <see cref="Unit"/>
    /// to be assigned the <see cref="BuildTask"/> task.
    /// </summary>
    public sealed class Build : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> builderHandles;
        private readonly Handle buildingTypeHandle;
        private readonly Point location;
        #endregion

        #region Constructors
        public Build(Handle factionHandle, IEnumerable<Handle> builderHandles, Handle buildingTypeHandle, Point location)
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
        #endregion

        #region Methods
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

            UnitType buildingType = (UnitType)match.World.UnitTypes.FromHandle(buildingTypeHandle);
            BuildingPlan plan = new BuildingPlan(buildingType, location);


            foreach (Handle unit in builderHandles)
            {
                Unit builder = (Unit)match.World.Entities.FromHandle(unit);
                builder.Task = new BuildTask(builder, plan);
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

        public static Build DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            IEnumerable<Handle> builderHandles = ReadLengthPrefixedHandleArray(reader);
            Handle buildingTypeHandle = ReadHandle(reader);
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            Point location = new Point(x, y);
            return new Build(factionHandle, builderHandles, buildingTypeHandle, location);
        }
        #endregion
        #endregion
    }
}
