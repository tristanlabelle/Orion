using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;

using ZoneAttackTask = Orion.GameLogic.Tasks.ZoneAttack;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Unit"/>s to move to a location and attack enemies on their way.
    /// </summary>
    public sealed class ZoneAttack : Command
    {

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

        #endregion

        #region Methods
        
        public override void Execute()
        {
            foreach (Unit striker in attackers)
                striker.Task = new ZoneAttackTask(striker, destination);
        }
        
        #endregion


    }
}
