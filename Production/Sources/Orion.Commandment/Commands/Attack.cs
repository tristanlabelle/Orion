using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using AttackTask = Orion.GameLogic.Tasks.Attack;

namespace Orion.Commandment.Commands
{
    public sealed class Attack : Command
    {
        #region Fields
        private readonly List<Unit> strikers;
        private readonly Unit enemy; 
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Attack"/> command from the faction which
        /// created the command and a sequence of <see cref="Unit"/>s for which the
        /// current <see cref="Task"/> should be attacked. 
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> that created this command.</param>
        /// <param name="units">
        /// The <see cref="Unit"/>s of that <see cref="Faction"/> which <see cref="Task"/>s are to be attacked.
        /// </param>
        public Attack(Faction faction, IEnumerable<Unit> strikers, Unit enemy)
            : base(faction)
        {
            Argument.EnsureNotNull(enemy, "enemy");
            Argument.EnsureNotNullNorEmpty(strikers, "strikers");
            if (strikers.Any(unit => unit.Faction != base.SourceFaction))
                throw new ArgumentException("Expected all units to be from the source faction.", "units");
            this.enemy = enemy; 
            this.strikers = strikers.Distinct().ToList();
        }
        #endregion
        #region Methods
        public override void Execute()
        {
            foreach (Unit striker in strikers)
                striker.Task = new AttackTask(striker, enemy);
        }
        #endregion
    }
}