using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic.Buildings
{
    class CommandCenter : Building
    {

        #region Constructors
        public CommandCenter(int maxHealthPoints, Vector2 position, World world)
            : base(maxHealthPoints, position, world)
        {
        }
        #endregion
    }
}
