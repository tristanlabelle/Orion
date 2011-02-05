using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Orion.Engine
{
    /// <summary>
    /// An attribute which associates a "changed" event to a property.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyChangedEventAttribute : Attribute
    {
        #region Fields
        private readonly string eventName;
        #endregion

        #region Constructors
        public PropertyChangedEventAttribute(string eventName)
        {
            Argument.EnsureNotNull(eventName, "eventName");

            this.eventName = eventName;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the event raised when the tagged property changes.
        /// </summary>
        public string EventName
        {
            get { return eventName; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Change event: " + eventName;
        }
        #endregion
    }
}
