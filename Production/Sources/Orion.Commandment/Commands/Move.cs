using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using MoveTask = Orion.GameLogic.Tasks.Move;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which assigns to a set of <see cref="Unit"/>s the
    /// <see cref="Task"/> to move to a destination.
    /// </summary>
    [Serializable]
    public sealed class Move : Command
    {
        #region Instance
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        private readonly Vector2 destination;
        #endregion
        
        #region Constructors
        public Move(Handle factionHandle, IEnumerable<Handle> unitHandles, Vector2 destination)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(unitHandles, "unitsToMove");

            this.unitHandles = unitHandles.Distinct().ToList().AsReadOnly();
            this.destination = destination;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return unitHandles; }
        }
        #endregion

        #region Methods
        public override void Execute(World world)
        {
            Argument.EnsureNotNull(world, "world");

            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)world.Entities.FindFromHandle(unitHandle);
                unit.Task = new MoveTask(unit, destination);
            }
        }

        public override string ToString()
        {
            return "[{0}] move to {1}".FormatInvariant(unitHandles.ToCommaSeparatedValues(), destination);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Move"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<Move>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Move command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteLengthPrefixedHandleArray(writer, command.unitHandles);
                writer.Write(command.destination.X);
                writer.Write(command.destination.Y);
            }

            protected override Move DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                var unitHandles = ReadLengthPrefixedHandleArray(reader);
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                Vector2 destination = new Vector2(x, y);
                return new Move(factionHandle, unitHandles, destination);
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
