using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.GameLogic.Buildings
{
    public class DefenseTower : Building
    {
        #region Fields
        private const float attackRange = 10;
        #endregion

        #region Constructors
        public DefenseTower(int maxHealthPoints, Vector2 position, World world)
            : base(maxHealthPoints, position, world)
        {

        }
        #endregion

        #region Properties
        public float Range
        {
            get { return attackRange; }
        }
        #endregion
    }
}
