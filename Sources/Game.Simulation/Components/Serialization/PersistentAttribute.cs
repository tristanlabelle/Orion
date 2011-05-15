using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Orion.Engine;

namespace Orion.Game.Simulation.Components.Serialization
{
    /// <summary>
    /// Indicates that an attribute is permanent to a component.
    /// </summary>
    /// <remarks>
    /// Properties with this attribute are always serialized when their owning component is. Therefore,
    /// these should indicate the minimum set of data a component needs to be usable. Transient properties,
    /// properties whose represented value change during the course of a game, should not have this attribute
    /// set.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal sealed class PersistentAttribute : Attribute
    {
        #region Fields
        private readonly bool isMandatory;
        #endregion

        #region Constructors
        public PersistentAttribute() { }

        public PersistentAttribute(bool mandatory)
        {
            this.isMandatory = mandatory;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if the property must be specified
        /// in the data being deserialized.
        /// </summary>
        public bool IsMandatory
        {
            get { return isMandatory; }
        }
        #endregion

        #region Methods
        public static bool IsPropertyMandatory(PropertyInfo property)
        {
            Argument.EnsureNotNull(property, "property");

            return property.GetCustomAttributes(typeof(PersistentAttribute), true)
                .Cast<PersistentAttribute>()
                .Any(a => a.isMandatory);
        }
        #endregion
    }
}
