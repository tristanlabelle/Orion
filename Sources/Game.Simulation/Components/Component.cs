using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Skills;
using System.Reflection;

namespace Orion.Game.Simulation.Components
{
    public abstract class Component
    {
        #region Fields
        private Entity entity;
        #endregion

        #region Constructors
        public Component(Entity entity)
        {
            this.entity = entity;
        }
        #endregion

        #region Properties
        public Entity Entity
        {
            get { return entity; }
        }
        #endregion

        #region Methods
        public virtual Stat GetStatBonus(EntityStat stat)
        {
            Type type = GetType();
            if (type != stat.ComponentType)
                return new Stat();

            PropertyInfo property = type.GetProperty(stat.Name, BindingFlags.Public);
            if (property == null)
                throw new InvalidComponentStatException(type, stat);

            if (stat.NumericType == StatType.Integer)
                return new Stat((int)property.GetValue(this, null));
            else
                return new Stat((float)property.GetValue(this, null));
        }
        #endregion
    }
}
