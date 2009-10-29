using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using OpenTK.Math;
using System.IO;

namespace Orion.Commandment.Commands
{
    class Build: Command
    {
        #region Instance
        #region Fields
        private readonly Unit constructor;
        private readonly Vector2 position;
        private readonly UnitType unitTobuild;
        #endregion

        #region Constructors
        /// <summary>
        /// Command implemented to build.
        /// </summary>
        /// <param name="selectedUnit">The Builder</param>
        /// <param name="position">Where To build</param>
        /// <param name="unitTobuild">What to build</param>
        public Build(Unit selectedUnit, Vector2 position,UnitType unitTobuild)
            : base(selectedUnit.Faction)
        {
            this.constructor = selectedUnit;
            this.unitTobuild = unitTobuild;
            Argument.EnsureNotNull(position, "position");
            this.position = position;
        }
        #endregion

        #region Methods
        public override void Execute()
        {
           
                constructor.Task = new Orion.GameLogic.Tasks.Build(constructor,position,unitTobuild);
        }
        #endregion

        #region Proprieties
        public override IEnumerable<Unit> UnitsInvolved
        {
            get
            {
                yield return constructor;
            }
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
            #region Properties
            public override byte ID
            {
                get { return 1; }
            }
            #endregion

            #region Methods
            protected override void SerializeData(Build command, BinaryWriter writer)
            {
                writer.Write(command.constructor.ID);
                writer.Write(command.position.X);
                writer.Write(command.position.Y);
                writer.Write(command.unitTobuild.ID);
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
