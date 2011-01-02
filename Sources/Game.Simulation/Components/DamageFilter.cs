using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    public sealed class DamageFilter
    {
        #region Fields
        private readonly Func<Entity, float> filter;
        private readonly string description;
        #endregion

        #region Constructors
        public DamageFilter(Func<Entity, float> function, string description)
        {
            this.filter = function;
            this.description = description;
        }
        #endregion

        #region Properties
        public Func<Entity, float> Apply
        {
            get { return filter; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return description;
        }
        #endregion
    }
}
