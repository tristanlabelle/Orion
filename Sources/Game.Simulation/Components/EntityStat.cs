using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Identifies a characteristic associated with an entity.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class EntityStat
    {
        #region Fields
        private readonly Type componentType;
        private readonly string componentName;
        private readonly string name;
        private readonly string fullName;
        private readonly string description;
        #endregion

        #region Constructors
        internal EntityStat(Type componentType, string name, string description)
        {
            Argument.EnsureNotNull(componentType, "skillType");
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureNotNull(description, "description");

            this.componentType = componentType;
            this.componentName = UnitSkill.GetTypeName(componentType);
            this.name = name;
            this.fullName = componentName + '.' + name;
            this.description = description;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of the component which defines this stat.
        /// </summary>
        public Type ComponentType
        {
            get { return componentType; }
        }

        /// <summary>
        /// Gets the name of the component which defines this stat.
        /// </summary>
        public string ComponentName
        {
            get { return componentName; }
        }

        /// <summary>
        /// Gets the name of this stat within its component.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the fully qualified name of this stat.
        /// </summary>
        public string FullName
        {
            get { return fullName; }
        }

        /// <summary>
        /// Gets a human-readable description of this stat.
        /// </summary>
        public string Description
        {
            get { return description; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return fullName;
        }
        #endregion
    }
}
