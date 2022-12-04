using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic.Tasks;
using Orion.GameLogic;

namespace Orion.Matchmaking.TowerDefense
{
    /// <summary>
    /// Implements the logic which makes a creep follow the path as it should.
    /// </summary>
    public sealed class CreepTask : Task
    {
        #region Fields
        private readonly CreepPath path;
        private int currentPointIndex;
        private MoveTask moveTask;
        #endregion

        #region Constructors
        public CreepTask(Unit creep, CreepPath path)
            : base(creep)
        {
            Argument.EnsureNotNull(path, "path");
            this.path = path;
            this.moveTask = new MoveTask(creep, path.Points[1]);
        }
        #endregion

        #region Properties
        public override bool HasEnded
        {
            get { return false; }
        }

        public override string Description
        {
            get { return "following creep path"; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (!moveTask.HasEnded)
            {
                moveTask.Update(step);
                return;
            }

            ++currentPointIndex;
            if (currentPointIndex == path.Points.Count - 1)
            {
                Unit.Suicide();
                return;
            }

            moveTask = new MoveTask(Unit, path.Points[currentPointIndex + 1]);
        }
        #endregion
    }
}
