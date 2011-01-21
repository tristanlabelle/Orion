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
        private readonly UnitType type;
        #endregion

        #region Constructors
        public Identity(Entity entity, UnitType type)
            : base(entity)
        {
            Argument.EnsureNotNull(type, "type");
            this.type = type;
        }
        #endregion

        #region Properties
        public UnitType Type
        {
            get { return type; }
        }
        #endregion
    }
}
