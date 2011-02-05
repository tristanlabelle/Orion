using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Skills;
using System.Reflection;
using Orion.Game.Simulation.Components.Serialization;
using System.Collections;

namespace Orion.Game.Simulation.Components
{
    public abstract class Component
    {
        #region Fields
        private static readonly Type[] constructorArguments = new Type[] { typeof(Entity) };
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
        public virtual void Update(SimulationStep step)
        { }

        public virtual Stat GetStatBonus(EntityStat stat)
        {
            Type type = GetType();
            if (type != stat.ComponentType)
                return new Stat();

            PropertyInfo property = type.GetProperty(stat.Name);
            if (property == null)
                throw new InvalidComponentStatException(type, stat);

            if (stat.NumericType == StatType.Integer)
                return new Stat((int)property.GetValue(this, null));
            else
                return new Stat((float)property.GetValue(this, null));
        }

        public Component Clone(Entity entity)
        {
            Type type = GetType();
            Component newInstance = type
                .GetConstructor(constructorArguments)
                .Invoke(new object[] { entity }) as Component;

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttributes(typeof(PersistentAttribute), true).Length == 0)
                    continue;

                object currentValue = property.GetValue(this, null);
                Type propertyType = currentValue.GetType();

                if (typeof(ICollection).IsAssignableFrom(propertyType))
                {
                    // if it's a collection, copy the contents
                    object newCollection = property.GetValue(newInstance, null);
                    Type typeArgument = propertyType.GetGenericArguments()[0];
                    MethodInfo copyCollection = typeof(Component)
                        .GetMethod("CopyCollection", BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(typeArgument);
                    copyCollection.Invoke(null, new object[] { currentValue, newCollection });
                }
                else
                {
                    // otherwise, set the value
                    property.SetValue(newInstance, currentValue, null);
                }
            }
            return newInstance;
        }

        private static void CopyCollection<T>(ICollection<T> from, ICollection<T> to)
        {
            foreach (T item in from) to.Add(item);
        }
        #endregion
    }
}
