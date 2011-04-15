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
        private bool isAwake;
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

        /// <summary>
        /// Gets a value indicating if this <see cref="Component"/> is awake,
        /// meaning that it is part of an <see cref="T:Entity"/> which is itself
        /// part of the <see cref="T:World"/>.
        /// </summary>
        protected bool IsAwake
        {
            get { return isAwake; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Allows this <see cref="Component"/> to update its logic for a frame.
        /// </summary>
        /// <param name="step">The frame's simulation step.</param>
        protected virtual void Update(SimulationStep step) { }

        /// <remarks>
        /// Proxy to the <see cref="Update"/> method invoked by <see cref="T:Entity"/>
        /// so the method can have protected visibility.
        /// </remarks>
        internal void InvokeUpdate(SimulationStep step)
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
        /// Performs <see cref="Component"/>-specific initialization logic.
        /// This gets called once the <see cref="Component"/> is part of an <see cref="T:Entity"/>
        /// which is itself part of a <see cref="T:World"/>.
        /// </summary>
        protected virtual void Wake() { }

        /// <remarks>
        /// Proxy to the <see cref="Wake"/> method invoked by <see cref="T:Entity"/>
        /// so the method can have protected visibility.
        /// </remarks>
        internal void InvokeWake()
        {
            Debug.Assert(!isAwake, "Waking an already awake component.");
            Wake();
            isAwake = true;
        }

        /// <summary>
        /// Performs <see cref="Component"/>-specific clean up logic.
        /// This gets called once the <see cref="Component"/> is removed from its <see cref="T:Entity"/>,
        /// or when its <see cref="T:Entity"/> is removed from the  <see cref="T:World"/>.
        /// </summary>
        protected virtual void Sleep() { }

        /// <remarks>
        /// Proxy to the <see cref="Sleep"/> method invoked by <see cref="T:Entity"/>
        /// so the method can have protected visibility.
        /// </remarks>
        internal void InvokeSleep()
        {
            Debug.Assert(isAwake, "Putting to sleep an already sleeping component.");
            Sleep();
            isAwake = false;
        }

        /// <summary>
        /// Copies the state of a given <see cref="Component"/>
        /// to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Component"/> from which state should be copied.</param>
        public void CopyFrom(Component other)
        {
            Argument.EnsureNotNull(other, "other");
            if (other.GetType() != GetType()) throw new ArgumentException("Cannot copy the state of a component of another type.");

            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                MethodInfo getter = property.GetGetMethod();
                if (getter == null || getter.GetParameters().Length > 0)
                    continue;

                Type propertyType = property.PropertyType;
                bool isGenericCollection = IsGenericCollection(propertyType, true);
                object value = property.GetValue(other, null);

                if (isGenericCollection)
                {
                    // if it's a collection, copy the contents
                    object newCollection = property.GetValue(this, null);
                    Type typeArgument = propertyType.GetGenericArguments()[0];
                    MethodInfo copyCollection = typeof(Component)
                        .GetMethod("CopyCollection", BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(typeArgument);
                    copyCollection.Invoke(null, new object[] { value, newCollection });
                }
                else if (property.GetSetMethod() != null)
                {
                    // otherwise, set the value
                    property.SetValue(this, value, null);
                }
            }
        }

        /// <summary>
        /// Clones this <see cref="Component"/> for another <see cref="T:Entity"/>.
        /// </summary>
        /// <param name="entity">The new <see cref="Component"/>'s host <see cref="T:Entity"/>.</param>
        /// <returns>The newly cloned <see cref="Component"/>.</returns>
        public Component Clone(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            Component clone = (Component)Activator.CreateInstance(GetType(), new object[] { entity });
            clone.CopyFrom(this);

            return clone;
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
