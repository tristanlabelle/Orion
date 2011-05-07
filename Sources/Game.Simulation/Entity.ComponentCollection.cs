using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    partial class Entity
    {
        /// <summary>
        /// The collection of an <see cref="Entity"/>'s <see cref="Component"/>s.
        /// </summary>
        public sealed class ComponentCollection : ICollection<Component>
        {
            #region Fields
            /// <summary>
            /// A component list shared between entities, which use it to update their components.
            /// This being static disallows reentrancy and concurrency.
            /// </summary>
            private static List<Component> sharedUpdatedComponentList = new List<Component>();

            private readonly Entity entity;
            private readonly Dictionary<Type, Component> components
                = new Dictionary<Type, Component>();

            /// <summary>
            /// The index of the currently updated component in <see cref="sharedUpdatedComponentList"/>.
            /// This is <c>-1</c> if the entity is not being updated,
            /// in which case <see cref="sharedUpdatedComponentList"/> might be in use by another entity.
            /// Used in the logic to prevent removed components to be updated in the frame.
            /// </summary>
            private int updatedComponentIndex = -1;

            /// <summary>
            /// Cache for the <see cref="Spatial"/> component as it is queried very often.
            /// </summary>
            private Spatial spatial;
            #endregion

            #region Constructors
            internal ComponentCollection(Entity entity)
            {
                Argument.EnsureNotNull(entity, "entity");

                this.entity = entity;
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the number of <see cref="Component"/>s this collection contains.
            /// </summary>
            public int Count
            {
                get { return components.Count; }
            }

            internal Spatial Spatial
            {
                get { return spatial; }
            }

            internal bool IsUpdating
            {
                get { return updatedComponentIndex != -1; }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Adds a given <see cref="Component"/> to this collection.
            /// </summary>
            /// <param name="component">The <see cref="Component"/> to be added.</param>
            public void Add(Component component)
            {
                Argument.EnsureNotNull(component, "component");

                if (component.Entity != entity)
                    throw new ArgumentException("Cannot add a component which doesn't belong to this entity.", "component");

                Type componentType = component.GetType();
                if (components.ContainsKey(componentType))
                {
                    string message = "The entity already has a component of type {0}.".FormatInvariant(componentType.FullName);
                    throw new ArgumentException(message, "component");
                }

                components.Add(componentType, component);
                if (componentType == typeof(Spatial)) spatial = (Spatial)component;

                if (entity.IsActive) component.InvokeActivate();
            }

            /// <summary>
            /// Removes a <see cref="Component"/> from this collection.
            /// </summary>
            /// <param name="component">The <see cref="Component"/> to be removed.</param>
            /// <returns>A value indicating if the component was found and removed.</returns>
            public bool Remove(Component component)
            {
                if (component == null) return false;

                Type componentType = component.GetType();
                Component actualComponent;
                return components.TryGetValue(componentType, out actualComponent)
                    && actualComponent == component
                    && Remove(componentType);
            }

            /// <summary>
            /// Removes a <see cref="Component"/> from this collection by its type.
            /// </summary>
            /// <param name="componentType">The type of the <see cref="Component"/> to be removed.</param>
            /// <returns>A value indicating if a <see cref="Component"/> with this type was found and removed.</returns>
            /// <remarks>
            /// All remove methods delegate to this one, so this is the proper place for bookkeeping.
            /// </remarks>
            public bool Remove(Type componentType)
            {
                Component component;
                if (!components.TryGetValue(componentType, out component))
                    return false;

                components.Remove(componentType);

                // In the case where an updated component removes another one,
                // prevent the removed component from being updated.
                if (IsUpdating)
                {
                    for (int i = updatedComponentIndex + 1; i < sharedUpdatedComponentList.Count; ++i)
                    {
                        if (sharedUpdatedComponentList[i] == component)
                        {
                            sharedUpdatedComponentList.RemoveAt(i);
                            break;
                        }
                    }
                }

                if (componentType == typeof(Spatial)) spatial = null;

                if (entity.IsActive) component.InvokeDeactivate();

                return true;
            }

            /// <summary>
            /// Removes a <see cref="Component"/> from this collection by its type.
            /// </summary>
            /// <typeparam name="TComponent">The type of the <see cref="Component"/> to be removed.</typeparam>
            /// <returns>A value indicating if a <see cref="Component"/> with this type was found and removed.</returns>
            public bool Remove<TComponent>() where TComponent : Component
            {
                return Remove(typeof(TComponent));
            }

            /// <summary>
            /// Removes all <see cref="Component"/>s from this collection.
            /// </summary>
            public void Clear()
            {
                while (components.Count > 0)
                {
                    Type componentType = components.Keys.First();
                    Remove(componentType);
                }
            }

            /// <summary>
            /// Finds a <see cref="Component"/> by its type.
            /// </summary>
            /// <param name="componentType">The type of the <see cref="Component"/> to be found.</param>
            /// <returns>The <see cref="Component"/> with that type.</returns>
            /// <exception cref="KeyNotFoundException">
            /// Thrown if this collection contains not <see cref="Component"/> of that type.
            /// </exception>
            public Component Get(Type componentType)
            {
                return components[componentType];
            }

            /// <summary>
            /// Finds a <see cref="Component"/> by its type.
            /// </summary>
            /// <typeparam name="TComponent">The type of the <see cref="Component"/> to be found.</typeparam>
            /// <returns>The <see cref="Component"/> with that type.</returns>
            /// <exception cref="KeyNotFoundException">
            /// Thrown if this collection contains not <see cref="Component"/> of that type.
            /// </exception>
            public TComponent Get<TComponent>() where TComponent : Component
            {
                return (TComponent)Get(typeof(TComponent));
            }

            /// <summary>
            /// Attempts to find a <see cref="Component"/> by its type.
            /// </summary>
            /// <param name="componentType">The type of the <see cref="Component"/> to be found.</param>
            /// <returns>
            /// The <see cref="Component"/> with that type, or <c>null</c> if the <see cref="Entity"/> has no such component.
            /// </returns>
            public Component TryGet(Type componentType)
            {
                Component component;
                components.TryGetValue(componentType, out component);
                return component;
            }

            /// <summary>
            /// Attempts to find a <see cref="Component"/> by its type.
            /// </summary>
            /// <typeparam name="TComponent">The type of the <see cref="Component"/> to be found.</typeparam>
            /// <returns>
            /// The <see cref="Component"/> with that type, or <c>null</c> if the <see cref="Entity"/> has no such component.
            /// </returns>
            public TComponent TryGet<TComponent>() where TComponent : Component
            {
                return (TComponent)TryGet(typeof(TComponent));
            }

            /// <summary>
            /// Determines if this collection contains a <see cref="Component"/> of a given type.
            /// </summary>
            /// <param name="componentType">The type of <see cref="Component"/> to be found.</param>
            /// <returns>A value indicating if such a <see cref="Component"/> can be found in this collection.</returns>
            public bool Has(Type componentType)
            {
                return components.ContainsKey(componentType);
            }

            /// <summary>
            /// Determines if this collection contains a <see cref="Component"/> of a given type.
            /// </summary>
            /// <typeparam name="TComponent">The <see cref="Component"/> type.</typeparam>
            /// <returns>A value indicating if such a <see cref="Component"/> can be found in this collection.</returns>
            public bool Has<TComponent>() where TComponent : Component
            {
                return components.ContainsKey(typeof(TComponent));
            }

            /// <summary>
            /// Gets a value indicating if a given <see cref="Component"/> can be found in this collection.
            /// </summary>
            /// <param name="component">The <see cref="Component"/> to be found.</param>
            /// <returns>A value indicating if the <see cref="Component"/> was found.</returns>
            public bool Contains(Component component)
            {
                return components.ContainsValue(component);
            }

            /// <summary>
            /// Gets an enumerator which enumerates the <see cref="Component"/>s in this collection.
            /// </summary>
            /// <returns>A new <see cref="Component"/> enumerator.</returns>
            public Dictionary<Type, Component>.ValueCollection.Enumerator GetEnumerator()
            {
                return components.Values.GetEnumerator();
            }

            /// <summary>
            /// Updates the <see cref="Component"/>s in this collection.
            /// </summary>
            /// <param name="step">The simulation step.</param>
            /// <remarks>
            /// This method's logic uses a static component list because
            /// a single entity can be updated at a time. Care is taken to handle
            /// the situation where a component being updated removes an other component,
            /// in which case that other component should not be updated afterwards.
            /// </remarks>
            internal void Update(SimulationStep step)
            {
                if (sharedUpdatedComponentList.Count > 0)
                    throw new InvalidOperationException("Reentrant or concurrent entity update.");

                try
                {
                    foreach (var component in components.Values)
                        sharedUpdatedComponentList.Add(component);

                    updatedComponentIndex = 0;
                    while (updatedComponentIndex < sharedUpdatedComponentList.Count)
                    {
                        sharedUpdatedComponentList[updatedComponentIndex].InvokeUpdate(step);
                        ++updatedComponentIndex;
                    }
                }
                finally
                {
                    updatedComponentIndex = -1;
                    sharedUpdatedComponentList.Clear();
                }
            }

            /// <summary>
            /// Prevents any other <see cref="Component"/>s from being updated if an update
            /// was in progress.
            /// </summary>
            internal void CancelUpdate()
            {
                if (IsUpdating) sharedUpdatedComponentList.Clear();
            }
            #endregion

            #region Explicit Members
            bool ICollection<Component>.IsReadOnly
            {
                get { return false; }
            }

            void ICollection<Component>.CopyTo(Component[] array, int arrayIndex)
            {
                components.Values.CopyTo(array, arrayIndex);
            }

            IEnumerator<Component> IEnumerable<Component>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }
    }
}
