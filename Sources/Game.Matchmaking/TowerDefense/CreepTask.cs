using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.TowerDefense
{
    /// <summary>
    /// Implements the logic which makes a creep follow the path as it should.
    /// </summary>
    public sealed class CreepTask : Task
    {
        #region Fields
        private readonly CreepWaveCommander commander;
        private int currentPointIndex;
        private MoveTask moveTask;
        #endregion

        #region Constructors
        public CreepTask(Unit creep, CreepWaveCommander commander)
            : base(creep)
        {
            Argument.EnsureNotNull(commander, "commander");

            this.commander = commander;
            this.moveTask = new MoveTask(creep, commander.Path.Points[1]);
        }
        #endregion

        #region Properties
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
            if (currentPointIndex == commander.Path.Points.Count - 1)
            {
                Unit.Suicide();
                commander.RaiseCreepLeaked();
                return;
            }

            Point targetPoint = commander.Path.Points[currentPointIndex + 1];
            Region targetRegion = new Region(targetPoint.X, targetPoint.Y, 1, 1);
            moveTask = MoveTask.ToNearRegion(Unit, targetRegion);
            moveTask.Update(step);
        }
        #endregion
    }
}
