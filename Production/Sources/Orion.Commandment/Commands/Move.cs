using System;
using System.Collections.Generic;
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
        private readonly List<Unit> units;
        private readonly Vector2 destination;
        #endregion
        
        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Move"/> command from the <see cref="Faction"/> that
        /// created the command, the <see cref="Unit"/>s involved and the destination to be reached.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> that created this command.</param>
        /// <param name="units">A sequence of <see cref="Unit"/>s involved in this command.</param>
        /// <param name="destination">The location of the destination of the movement.</param>
        public Move(Faction faction, IEnumerable<Unit> units, Vector2 destination)
            : base(faction)
        {
            Argument.EnsureNotNullNorEmpty(units, "unitsToMove");
            if (units.Any(unit => unit.Faction != base.SourceFaction))
                throw new ArgumentException("Expected all units to be from the source faction.", "units");
            
            this.units = units.Distinct().ToList();
            this.destination = destination;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the sequence of <see cref="Unit"/>s participating in this command.
        /// </summary>
        public IEnumerable<Unit> Units
        {
            get { return units; }
        }

        /// <summary>
        /// Gets the location of the destination of this movement.
        /// </summary>
        public Vector2 Destination
        {
            get { return destination; }
        }

        public override IEnumerable<Entity> EntitiesInvolved
        {
            get { return units.Cast<Entity>(); }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit unit in units)
                unit.Task = new MoveTask(unit, destination);
        }

        public override string ToString()
        {
            return "[{0}] move to {1}".FormatInvariant(units.ToCommaSeparatedValues(), destination);
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
                writer.Write(command.SourceFaction.Handle.Value);
                writer.Write(command.units.Count);
                foreach (Unit unit in command.Units)
                    writer.Write(unit.Handle.Value);
                writer.Write(command.Destination.X);
                writer.Write(command.Destination.Y);
            }

            protected override Move DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                Unit[] units = ReadLengthPrefixedUnitArray(reader, world);
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                Vector2 destination = new Vector2(x, y);
                return new Move(sourceFaction, units, destination);
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
