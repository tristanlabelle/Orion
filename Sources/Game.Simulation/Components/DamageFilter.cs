using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    public class DamageFilter
    {
        #region Fields
        private Func<Entity, bool> applies;
        private int additionalDamage;
        private string description;
        #endregion

        #region Constructors
        public DamageFilter(Func<Entity, bool> applies, int additionalDamage, string description)
        {
            this.additionalDamage = additionalDamage;
            this.applies = applies;
            this.description = description;
        }
        #endregion

        #region Properties
        public Func<Entity, bool> Applies
        {
            get { return applies; }
        }

        public int AdditionalDamage
        {
            get { return additionalDamage; }
        }

        public string Description
        {
            get { return description; }
        }
        #endregion
    }
}
