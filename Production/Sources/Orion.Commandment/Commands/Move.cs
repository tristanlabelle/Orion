using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

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
    [SerializableCommand(2)]
    public sealed class Move : Command
    {
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

        #region Methods
        public override void Execute()
        {
            foreach (Unit unit in units)
                unit.Task = new MoveTask(unit, destination);
        }
        #endregion
    }
}
