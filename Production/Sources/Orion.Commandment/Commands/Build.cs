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
        private readonly Unit builder;
        private readonly Vector2 position;
        private readonly UnitType buildingType;
        #endregion

        #region Constructors
        /// <summary>
        /// Command implemented to build.
        /// </summary>
        /// <param name="builder">The Builder</param>
        /// <param name="position">Where To build</param>
        /// <param name="buildingType">What to build</param>
        public Build(Unit builder, Vector2 position, UnitType buildingType)
            : base(builder.Faction)
        {
            Argument.EnsureNotNull(builder, "builder");
            Argument.EnsureNotNull(buildingType, "buildingType");
            this.builder = builder;
            this.buildingType = buildingType;
            Argument.EnsureNotNull(position, "position");
            this.position = position;
        }
        #endregion

        #region Properties
        public override IEnumerable<Entity> EntitiesInvolved
        {
            get { yield return builder; }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            builder.Task = new BuildTask(builder, position, buildingType);
        }

        public override string ToString()
        {
            return "{0} build {1} at {2}".FormatInvariant(builder, buildingType, position);
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
                writer.Write(command.builder.ID);
                writer.Write(command.position.X);
                writer.Write(command.position.Y);
                writer.Write(command.buildingType.ID);
            }

            protected override Build DeserializeData(BinaryReader reader, World world)
            {
                Unit constructor = ReadUnit(reader, world);
                Vector2 position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                UnitType unitTobuild = ReadUnitType(reader, world);
                return new Build(constructor, position, unitTobuild);
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
