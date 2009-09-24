using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.GameLogic.Tasks;

using OpenTK.Math;

namespace Orion.Commandment.Commands
{
    public class Move : Command
    {

        #region Fields
        List<Unit> unitsToMove;
        Vector2 destination;

        #endregion
        

        #region Constructors

        public Move(Faction sourceFaction, IEnumerable<Unit> unitsToMove, Vector2 destination)
            : base(sourceFaction)
        {
            Argument.EnsureNotNullNorEmpty(unitsToMove, "unitsToMove");
            if (unitsToMove.Any(unit => unit.Faction != base.SourceFaction))
                throw new ArgumentException("Expected all units to be from the source faction.", "unitsToMove");
            

            this.unitsToMove = unitsToMove.Distinct().ToList();

            this.destination = destination;

        }

        #endregion

        #region Events

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        /// Assign task to all unit for this command
        /// </summary>
        public override void Execute()
        {

            foreach (Unit unit in unitsToMove)
            {
                unit.Task = new Orion.GameLogic.Tasks.Move(unit,destination);
            }
        }

        #endregion
       

        
    }
}
