using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.GameLogic.Tasks;
using OpenTK;

namespace Orion.Commandment.Commands
{
    public class Move : Command
    {

        #region Fields
        List<Unit> unitsToMove;
        Vector2 destination;

        #endregion
        public Move(IEnumerable<Unit> unitsToMove, Vector2 destination)
        {
            Argument.EnsureNotNullNorEmpty(unitsToMove, "unitsToMove");

            this.unitsToMove = unitsToMove.Distinct().ToList();

            this.destination = destination;

        }

        #region Constructors

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
