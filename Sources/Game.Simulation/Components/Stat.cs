using System;
using System.ComponentModel;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Identifies a characteristic associated with an entity.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class Stat
    {
        #region Fields
        private readonly Type componentType;
        private readonly StatType type;
        private readonly string name;
        private readonly string fullName;
        #endregion

        #region Constructors
        public Stat(Type componentType, StatType type, string name)
        {
            Argument.EnsureNotNull(componentType, "componentType");
            Argument.EnsureDefined(type, "type");
            Argument.EnsureNotNull(name, "name");

            this.type = type;
            this.componentType = componentType;
            this.name = name;
            this.fullName = componentType.Name + "." + name;
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
        /// Gets the type in which are the values of this stat represented.
        /// </summary>
        public StatType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the name of this stat within its component.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the full name of this <see cref="Stat"/>, in the form <c>ComponentName.StatName</c>.
        /// </summary>
        public string FullName
        {
            get { return fullName; }
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
