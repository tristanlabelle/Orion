using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Abstract base class for <see cref="Entity"/> components.
    /// Components provide behavior and/or state to the <see cref="Entity"/> to which they are attached.
    /// </summary>
    [Serializable]
    public abstract class Component
    {
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
        /// <summary>
        /// Allows this <see cref="Component"/> to update its logic for a frame.
        /// </summary>
        /// <param name="step">The frame's simulation step.</param>
        protected virtual void Update(SimulationStep step) { }

        /// <remarks>Invoked by <see cref="Entity"/>.</remarks>
        internal void DoUpdate(SimulationStep step)
        {
            Update(step);
        }

        /// <summary>
        /// Gets the bonus this <see cref="Component"/> provides to a given <see cref="Stat"/>.
        /// </summary>
        /// <param name="stat">The <see cref="Stat"/> for which the bonus is to be computed.</param>
        /// <returns>The value of the bonus for that <see cref="Stat"/>.</returns>
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

        /// <summary>
        /// Invoked after this <see cref="Component"/> has been added to its host <see cref="T:Entity"/>.
        /// </summary>
        protected virtual void OnAdded() { }

        /// <remarks>Invoked by <see cref="T:Entity"/>.</remarks>
        internal void NotifyAdded()
        {
            OnAdded();
        }

        /// <summary>
        /// Invoked after this <see cref="Component"/> has been removed from its host <see cref="T:Entity"/>.
        /// </summary>
        protected virtual void OnRemoved() { }

        /// <remarks>Invoked by <see cref="T:Entity"/>.</remarks>
        internal void NotifyRemoved()
        {
            OnRemoved();
        }

        public Component Clone(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            Type type = GetType();
            Component newInstance = type
                .GetConstructor(constructorArguments)
                .Invoke(new object[] { entity }) as Component;

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                MethodInfo getter = property.GetGetMethod();
                if (getter == null || getter.GetParameters().Length > 0)
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
                else if (property.GetSetMethod() != null)
                {
                    // otherwise, set the value
                    property.SetValue(newInstance, currentValue, null);
                }
            }
            return newInstance;
        }
        #endregion

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
    }
}
