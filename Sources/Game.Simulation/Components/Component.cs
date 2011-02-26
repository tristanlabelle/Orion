using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Skills;
using System.Reflection;
using Orion.Game.Simulation.Components.Serialization;
using System.Collections;
using System.Diagnostics;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Abstract base class for <see cref="Entity"/> components.
    /// Components provide behavior and/or state to the <see cref="Entity"/> to which they are attached.
    /// </summary>
    public abstract class Component
    {
        #region Static
        #region Methods
        public static T Clone<T>(T component, Entity entity) where T : Component
        {
            return (T)component.Clone(entity);
        }

        /// <summary>
        /// Helper method to copy items between two collections
        /// </summary>
        /// <remarks>
        /// Used via reflection by <see cref="M:Clone"/>.
        /// </remarks>
        private static void CopyCollection<T>(ICollection<T> from, ICollection<T> to)
        {
            foreach (T item in from) to.Add(item);
        }

        private static bool IsGenericCollection(Type type, bool checkInterfaces)
        {
            if (type.IsGenericTypeDefinition) return type == typeof(ICollection<>);
            if (type.IsGenericType) return type.GetGenericTypeDefinition() == typeof(ICollection<>);
            return checkInterfaces && type.GetInterfaces().Any(interfaceType => IsGenericCollection(interfaceType, false));
        }
        #endregion
        #endregion

        #region Fields
        private static readonly Type[] constructorArguments = new Type[] { typeof(Entity) };
        private readonly Entity entity;
        #endregion

        #region Constructors
        public Component(Entity entity)
        {
            this.entity = entity;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Entity"/> hosting this <see cref="Component"/>.
        /// </summary>
        public Entity Entity
        {
            get { return entity; }
        }

        protected World World
        {
            get { return entity.World; }
        }
        #endregion

        #region Methods
        public virtual void Update(SimulationStep step) { }

        public virtual StatValue GetStatBonus(Stat stat)
        {
            Type type = GetType();
            if (type != stat.ComponentType) return StatValue.CreateZero(stat.Type);

            PropertyInfo property = type.GetProperty(stat.Name);
            if (property == null)
            {
                Debug.Fail("Component {0} defines stat {1} but does not implement a property for it."
                    .FormatInvariant(GetType().FullName, stat.Name));
                return StatValue.CreateZero(stat.Type);
            }

            if (stat.Type == StatType.Integer)
                return StatValue.CreateInteger((int)property.GetValue(this, null));
            else
                return StatValue.CreateReal((float)property.GetValue(this, null));
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

                Type propertyType = property.PropertyType;
                bool isGenericCollection = IsGenericCollection(propertyType, true);
                object currentValue = property.GetValue(this, null);

                if (isGenericCollection)
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
        #endregion
    }
}
