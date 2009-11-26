using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Math;
using Orion.GameLogic;
using BuildTask = Orion.GameLogic.Tasks.Build;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes a <see cref="Unit"/>
    /// to be assigned the <see cref="BuildTask"/> task.
    /// </summary>
    public sealed class Build : Command
    {
        #region Fields
        private readonly Handle builderHandle;
        private readonly Handle buildingTypeHandle;
        private readonly Point location;
        #endregion

        #region Constructors
        public Build(Handle factionHandle, Handle builderHandle, Handle buildingTypeHandle, Point location)
            : base(factionHandle)
        {
            this.builderHandle = builderHandle;
            this.buildingTypeHandle = buildingTypeHandle;
            this.location = location;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { yield return builderHandle; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && IsValidEntityHandle(world, builderHandle)
                && IsValidUnitTypeHandle(world, buildingTypeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Unit builder = (Unit)match.World.Entities.FromHandle(builderHandle);
            UnitType buildingType = (UnitType)match.World.UnitTypes.FromHandle(buildingTypeHandle);
            builder.Task = new BuildTask(builder, buildingType, location);
        }

        public override string ToString()
        {
            return "Faction {0} builds {1} with {2} at {3}"
                .FormatInvariant(FactionHandle, buildingTypeHandle, builderHandle, location);
        }

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteHandle(writer, builderHandle);
            WriteHandle(writer, buildingTypeHandle);
            writer.Write((short)location.X);
            writer.Write((short)location.Y);
        }

        public static Build DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle builderHandle = ReadHandle(reader);
            Handle buildingTypeHandle = ReadHandle(reader);
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            Point location = new Point(x, y);
            return new Build(factionHandle, builderHandle, buildingTypeHandle, location);
        }
        #endregion
        #endregion
    }
}
