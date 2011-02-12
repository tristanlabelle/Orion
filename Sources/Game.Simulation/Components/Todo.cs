using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    public class Todo : Component
    {
        #region Fields
        private TaskQueue queue;
        #endregion

        #region Constructors
        public Todo(Entity e)
            : base(e)
        {
            queue = new TaskQueue((Unit)e);
        }
        #endregion

        #region Properties
        public TaskQueue Queue
        {
            get { return queue; }
        }

        [Transient]
        public bool IsIdle
        {
            get { return queue.IsEmpty; }
        }
        #endregion

        #region Methods
        public override void Update(SimulationStep step)
        {
            queue.Update(step);
        }
        #endregion
    }
}
