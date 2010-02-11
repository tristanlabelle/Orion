using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic.Buildings
{
    public class CommandCenter : Building
    {
        #region Fields
        private List<Task> taskQueue;
        #endregion

        #region Constructors
        public CommandCenter(int maxHealthPoints, Vector2 position, World world)
            : base(maxHealthPoints, position, world)
        {

        }
        #endregion

        #region Methods
        public void QueueTask(Task task)
        {
            taskQueue.Add(task);
        }
        #endregion
    }
}
