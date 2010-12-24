using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine
{
    /// <summary>
    /// Interface for objects which can notify listeners of changes in the value of its properties.
    /// </summary>
    public interface IPropertyChangedNotifier
    {
        /// <summary>
        /// Raised when the value of a property changes.
        /// The first argument is the event source and the second argument is the name of the property which changed.
        /// </summary>
        event Action<object, string> PropertyChanged;
    }
}
