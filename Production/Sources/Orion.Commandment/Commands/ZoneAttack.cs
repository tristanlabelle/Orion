using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using ZoneAttackTask = Orion.GameLogic.Tasks.ZoneAttack;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Unit"/>s
    /// to move to a location and attack enemies on their way.
    /// </summary>
    public sealed class ZoneAttack : Command
    {
        #region Instance
        #region Fields
        private readonly List<Unit> attackers;
        private readonly Vector2 destination; 
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ZoneAttack"/> command from the faction which
        /// created the command and a sequence of <see cref="Unit"/>s for which the
        /// current <see cref="Task"/> should be attacked. 
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> that created this command.</param>
        /// <param name="attackers">The <see cref="Unit"/>s of that <see cref="Faction"/> which should attack.</param>
        /// <param name="destination">The location of the destination of the movement.</param>
        public ZoneAttack(Faction faction, IEnumerable<Unit> attackers, Vector2 destination)
            : base(faction)
        {
            Argument.EnsureNotNull(destination, "destination");
            Argument.EnsureNotNullNorEmpty(attackers, "attackers");

            this.destination = destination;
            this.attackers = attackers.Distinct().ToList();

            if (this.attackers.Any(unit => unit.Faction != base.SourceFaction))
                throw new ArgumentException("Expected all units to be from the source faction.", "units");
        }
        
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of attacking <see cref="Unit"/>s.
        /// </summary>
        public int AttackerCount
        {
            get { return attackers.Count; }
        }

        /// <summary>
        /// Gets the sequence of attacking <see cref="Unit"/>s.
        /// </summary>
        public IEnumerable<Unit> Attackers
        {
            get { return attackers; }
        }

        /// <summary>
        /// Gets the destination of this movement.
        /// </summary>
        public Vector2 Destination
        {
            get { return destination; }
        }

        public override IEnumerable<Unit> UnitsInvolved
        {
            get
            {
                foreach (Unit unit in attackers)
                    yield return unit;
            }
        }
  

        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit attacker in attackers)
                attacker.Task = new ZoneAttackTask(attacker, destination);
        }

        public override string ToString()
        {
            return "[{0}] zone attack to {1}".FormatInvariant(attackers.ToCommaSeparatedValues(), destination);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="ZoneAttack"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<ZoneAttack>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(ZoneAttack command, BinaryWriter writer)
            {
                writer.Write(command.SourceFaction.ID);
                writer.Write(command.AttackerCount);
                foreach (Unit unit in command.Attackers)
                    writer.Write(unit.ID);
                writer.Write(command.Destination.X);
                writer.Write(command.Destination.Y);
            }

            protected override ZoneAttack DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                Unit[] attackers = ReadLengthPrefixedUnitArray(reader, world);
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                Vector2 destination = new Vector2(x, y);
                return new ZoneAttack(sourceFaction, attackers, destination);
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
