using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    public class Identity : Component
    {
        #region Fields
        private UnitType type;
        #endregion

        #region Constructors
        public Identity(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        public UnitType Type
        {
            get { return type; }
            set { type = value; }
        }
        #endregion
    }
}
