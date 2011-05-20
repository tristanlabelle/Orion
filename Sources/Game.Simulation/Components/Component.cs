using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Orion.Engine;
using System.Reflection.Emit;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Abstract base class for <see cref="Entity"/> components.
    /// Components provide behavior and/or state to the <see cref="Entity"/> to which they are attached.
    /// </summary>
    [Serializable]
    public abstract class Component
    {
        #region StatGetterCacheKey Structure
        /// <summary>
        /// A component type and stat tuple used as a key in the stat value getter dictionary.
        /// </summary>
        private struct StatGetterCacheKey : IEquatable<StatGetterCacheKey>
        {
            public Type ComponentType;
            public Stat Stat;

            public bool Equals(StatGetterCacheKey other)
            {
                return other.ComponentType == ComponentType && other.Stat == Stat;
            }

            public override bool Equals(object obj)
            {
                return obj is StatGetterCacheKey && Equals((StatGetterCacheKey)obj);
            }

            public override int GetHashCode()
            {
                return ComponentType.GetHashCode() ^ Stat.GetHashCode();
            }
        }
        #endregion

        #region Fields
        // Null stat value getters that always return zero.
        private static readonly Func<Component, StatValue> zeroIntegerStatValueGetter
            = component => StatValue.IntegerZero;
        private static readonly Func<Component, StatValue> zeroRealStatValueGetter
            = component => StatValue.RealZero;

        /// <summary>
        /// The stat value getter cache.
        /// This associates a delegate returning the stat value for a given component instance
        /// to every (component type, stat) tuple.
        /// </summary>
        private static readonly Dictionary<StatGetterCacheKey, Func<Component, StatValue>> statValueGetterCache
            = new Dictionary<StatGetterCacheKey, Func<Component, StatValue>>();

        private static readonly Type[] valueGetterMethodParameterTypes = new[] { typeof(Component) };
        private static readonly MethodInfo createIntegerStatValueMethod = typeof(StatValue).GetMethod("CreateInteger");
        private static readonly MethodInfo createRealStatValueMethod = typeof(StatValue).GetMethod("CreateReal");

        private readonly Entity entity;
        private bool isActive;
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
        /// Gets a value indicating if this <see cref="Component"/> is active,
        /// meaning that it is part of an <see cref="T:Entity"/> which is itself
        /// part of the <see cref="T:World"/>.
        /// </summary>
        protected bool IsActive
        {
            get { return isActive; }
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
            Debug.Assert(entity.IsAlive && entity.IsActive && isActive,
                "A component was updated when it wasn't active.");
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

            // The default way to retrieve a stat bonus is to get the value from a property
            // with the same name as the stat. Using reflection yields a performance which takes
            // about 15% of the simulation update time, so this code instead caches delegates
            // that retrieve directly the value of the properties for each (component type, stat) tuple.

            StatGetterCacheKey key = new StatGetterCacheKey
            {
                ComponentType = type,
                Stat = stat
            };

            // This is not thread-safe!
            Func<Component, StatValue> valueGetterDelegate;
            if (!statValueGetterCache.TryGetValue(key, out valueGetterDelegate))
            {
                PropertyInfo property = type.GetProperty(stat.Name);
                MethodInfo propertyGetter = property == null
                    ? null : property.GetGetMethod();
                if (propertyGetter == null)
                {
                    Debug.Fail("Component {0} defines stat {1} but does not implement a gettable property for it."
                        .FormatInvariant(GetType().FullName, stat.Name));

                    // Insert a dummy value getter which always returns 0
                    valueGetterDelegate = stat.Type == StatType.Integer ?
                        zeroIntegerStatValueGetter : zeroRealStatValueGetter;
                }
                else
                {
                    // Warning! Black magic ahead :D
                    // Generate a method returning the StatValue for a ComponentInstance
                    DynamicMethod generatedValueGetterMethod = new DynamicMethod(
                        "GeneratedGet" + type.Name + stat.Name + "StatValue",
                        typeof(StatValue), valueGetterMethodParameterTypes);

                    // Load the component instance argument
                    ILGenerator ilGenerator = generatedValueGetterMethod.GetILGenerator();
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    // Cast it to its actual derived component type
                    ilGenerator.Emit(OpCodes.Castclass, type);
                    // Retrieve the value of the stat property
                    ilGenerator.Emit(OpCodes.Callvirt, propertyGetter);
                    // Wrap it in a StatValue structure
                    ilGenerator.Emit(OpCodes.Call, stat.Type == StatType.Integer
                        ? createIntegerStatValueMethod : createRealStatValueMethod);
                    // Return it
                    ilGenerator.Emit(OpCodes.Ret);

                    // Make a delegate out of the generated dynamic method
                    valueGetterDelegate = (Func<Component, StatValue>)
                        generatedValueGetterMethod.CreateDelegate(typeof(Func<Component, StatValue>), null);
                }

                statValueGetterCache.Add(key, valueGetterDelegate);
            }

            return valueGetterDelegate(this);
        }

        public static StatValue Foo(Component component)
        {
            return StatValue.CreateInteger(((Health)component).MaxValue);
        }

        /// <summary>
        /// Performs <see cref="Component"/>-specific initialization logic.
        /// This gets called once the <see cref="Component"/> is part of an <see cref="T:Entity"/>
        /// which is itself part of a <see cref="T:World"/>.
        /// </summary>
        protected virtual void Activate() { }

        /// <remarks>
        /// Proxy to the <see cref="Activate"/> method invoked by <see cref="T:Entity"/>
        /// so the method can have protected visibility.
        /// </remarks>
        internal void InvokeActivate()
        {
            if (isActive)
            {
                Debug.Fail("Attempted to activate an already active component.");
                return;
            }

            Activate();
            isActive = true;
        }

        /// <summary>
        /// Performs <see cref="Component"/>-specific clean up logic.
        /// This gets called once the <see cref="Component"/> is removed from its <see cref="T:Entity"/>,
        /// or when its <see cref="T:Entity"/> is removed from the  <see cref="T:World"/>.
        /// </summary>
        protected virtual void Deactivate() { }

        /// <remarks>
        /// Proxy to the <see cref="Deactivate"/> method invoked by <see cref="T:Entity"/>
        /// so the method can have protected visibility.
        /// </remarks>
        internal void InvokeDeactivate()
        {
            if (!isActive)
            {
                Debug.Fail("Attempted to deactivate an already inactive component.");
                return;
            }

            Deactivate();
            isActive = false;
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
