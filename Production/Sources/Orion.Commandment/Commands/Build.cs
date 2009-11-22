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
        private readonly Vector2 position;
        private readonly Handle buildingTypeHandle;
        #endregion

        #region Constructors
        public Build(Handle factionHandle, Handle builderHandle, Handle buildingTypeHandle, Vector2 position)
            : base(factionHandle)
        {
            this.builderHandle = builderHandle;
            this.buildingTypeHandle = buildingTypeHandle;
            this.position = position;
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
            builder.Task = new BuildTask(builder, buildingType, position);
        }

        public override string ToString()
        {
            return "{0} build {1} at {2}".FormatInvariant(builderHandle, buildingTypeHandle, position);
        }

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteHandle(writer, builderHandle);
            WriteHandle(writer, buildingTypeHandle);
            writer.Write(position.X);
            writer.Write(position.Y);
        }

        public static Build DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle builderHandle = ReadHandle(reader);
            Handle buildingTypeHandle = ReadHandle(reader);
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            Vector2 position = new Vector2(x, y);
            return new Build(factionHandle, builderHandle, buildingTypeHandle, position);
        }
        #endregion
        #endregion
    }
}
