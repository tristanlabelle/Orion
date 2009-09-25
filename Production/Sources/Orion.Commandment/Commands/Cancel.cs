using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using Orion.GameLogic.Tasks;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which cancels the current <see cref="Task"/> of a set of <see cref="Unit"/>s.
    /// </summary>
    [Serializable]
    public sealed class Cancel : Command
    {
        #region Fields
        private readonly List<Unit> units;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Cancel"/> command from the faction which
        /// created the command and a sequence of <see cref="Unit"/>s for which the
        /// current <see cref="Task"/> should be canceled.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> that created this command.</param>
        /// <param name="units">
        /// The <see cref="Unit"/>s of that <see cref="Faction"/> which <see cref="Task"/>s are to be canceled.
        /// </param>
        public Cancel(Faction faction, IEnumerable<Unit> units)
            : base(faction)
        {
            Argument.EnsureNotNullNorEmpty(units, "units");
            if (units.Any(unit => unit.Faction != base.SourceFaction))
                throw new ArgumentException("Expected all units to be from the source faction.", "units");
            
            this.units = units.Distinct().ToList();
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Unit"/>s affected by this <see cref="Command"/>.
        /// </summary>
        public IEnumerable<Unit> Units
        {
            get { return units; }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit unit in units)
                unit.Task = Stand.Instance;
        }
        #endregion
    }
}
