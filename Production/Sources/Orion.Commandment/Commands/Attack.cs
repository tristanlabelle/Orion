using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using AttackTask = Orion.GameLogic.Tasks.Attack;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Unit"/>s
    /// to attack another <see cref="Unit"/>.
    /// </summary>
    public sealed class Attack : Command
    {
        #region Instance
        #region Fields
        private readonly List<Unit> attackers;
        private readonly Unit target; 
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Attack"/> command from the faction which
        /// created the command and a sequence of <see cref="Unit"/>s for which the
        /// current <see cref="Task"/> should be attacked. 
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> that created this command.</param>
        /// <param name="attackers">
        /// The <see cref="Unit"/>s of that <see cref="Faction"/> which should attack.
        /// </param>
        /// <param name="target">The target <see cref="Unit"/> to be attacked.</param>
        public Attack(Faction faction, IEnumerable<Unit> attackers, Unit target)
            : base(faction)
        {
            Argument.EnsureNotNull(target, "target");
            Argument.EnsureNotNullNorEmpty(attackers, "attackers");

            this.target = target;
            this.attackers = attackers.Distinct().ToList();

            if (this.attackers.Any(unit => unit.Faction != base.SourceFaction))
                throw new ArgumentException("Expected all units to be from the source faction.", "units");

            if (this.attackers.Contains(target))
                throw new ArgumentException("The attack target cannot be one of the attackers.", "target");
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
        /// Gets the target <see cref="Unit"/> to be attacked.
        /// </summary>
        public Unit Target
        {
            get { return target; }
        }

        public override IEnumerable<Entity> EntitiesInvolved
        {
            get
            {
                foreach (Unit attacker in attackers)
                    yield return attacker;
                yield return target;
            }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit attacker in attackers)
                attacker.Task = new AttackTask(attacker, target);
        }

        public override string ToString()
        {
            return "[{0}] attack {1}".FormatInvariant(attackers.ToCommaSeparatedValues(), target);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Attack"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<Attack>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Attack command, BinaryWriter writer)
            {
                writer.Write(command.SourceFaction.ID);
                writer.Write(command.AttackerCount);
                foreach (Unit attacker in command.Attackers)
                    writer.Write(attacker.ID);
                writer.Write(command.Target.ID);
            }

            protected override Attack DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                Unit[] attackers = ReadLengthPrefixedUnitArray(reader, world);
                Unit target = ReadUnit(reader, world);
                return new Attack(sourceFaction, attackers, target);
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