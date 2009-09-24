using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.GameLogic.Tasks;

namespace Orion.Commandment.Commands
{
    class Cancel : Command
    {
           #region Fields
        List<Unit> unitsToMove;

        #endregion
        

        #region Constructors
        /// <summary>
        /// Check the params and assigned them 
        /// </summary>
        /// <param name="sourceFaction">the faction that execute this command</param>
        /// <param name="unitsToCancelTask">unit of the faction to execute the task</param>
        public Cancel(Faction sourceFaction, IEnumerable<Unit> unitsToCancelTask)
            : base(sourceFaction)
        {
            Argument.EnsureNotNullNorEmpty(unitsToCancelTask, "unitsToCancelTask");
            if (unitsToCancelTask.Any(unit => unit.Faction != base.SourceFaction))
                throw new ArgumentException("Expected all units to be from the source faction.", "unitsToCancelTask");
            

            this.unitsToMove = unitsToCancelTask.Distinct().ToList();
        }

        #endregion

        #region Events

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        /// Assign task to be stand (when assigned Stand task.abord is called)
        /// </summary>
        public override void Execute()
        {
            foreach (Unit unit in unitsToMove)
            {
                unit.Task = new Orion.GameLogic.Tasks.Stand();
            }
        }

        #endregion
       

    }
}
