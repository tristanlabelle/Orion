using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Math;
using Orion.GameLogic;
using BuildTask = Orion.GameLogic.Tasks.Build;

namespace Orion.Commandment.Commands
{
    public sealed class Build : Command
    {
        #region Instance
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
        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Unit builder = (Unit)match.World.Entities.FindFromHandle(builderHandle);
            UnitType buildingType = (UnitType)match.World.UnitTypes.FromHandle(buildingTypeHandle);
            builder.Task = new BuildTask(builder, buildingType, position);
        }

        public override string ToString()
        {
            return "{0} build {1} at {2}".FormatInvariant(builderHandle, buildingTypeHandle, position);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Attack"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<Build>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Build command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteHandle(writer, command.builderHandle);
                WriteHandle(writer, command.buildingTypeHandle);
                writer.Write(command.position.X);
                writer.Write(command.position.Y);
            }

            protected override Build DeserializeData(BinaryReader reader)
            {
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

            #region Static
            #region Fields
            /// <summary>
            /// A globally available static instance of this class.
            /// </summary>
            public static readonly Serializer Instance = new Serializer();
            #endregion
            #endregion
        }
        #endregion
    }
}
