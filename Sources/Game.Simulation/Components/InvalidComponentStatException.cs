using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// An exception that is thrown when an attempt was made to query a stat on
    /// a skill to which it is not associated.
    /// </summary>
    [Serializable]
    public sealed class InvalidComponentStatException : ArgumentException
    {
        #region Fields
        private readonly Type componentType;
        private readonly object stat;
        #endregion

        #region Constructors
        public InvalidComponentStatException(Type componentType, object stat)
        {
            Argument.EnsureNotNull(componentType, "skillType");
            Argument.EnsureNotNull(stat, "stat");

            this.componentType = componentType;
            this.stat = stat;
        }
        #endregion

        #region Properties
        public Type ComponentType
        {
            get { return componentType; }
        }

        public object Stat
        {
            get { return stat; }
        }

        public override string ParamName
        {
            get { return "stat"; }
        }

        public override string Message
        {
            get { return "Invalid stat {0} for {1}.".FormatInvariant(stat, componentType.Name); }
        }
        #endregion

        #region Methods
        #endregion
    }
}
