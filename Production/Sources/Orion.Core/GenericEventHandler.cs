using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    /// <summary>
    /// Delegate for a method which can handle generic events with arguments.
    /// </summary>
    /// <typeparam name="TSender">The type of the object that raised the event.</typeparam>
    /// <typeparam name="TEventArgs">The type of the arguments associated with the event.</typeparam>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The arguments associated with the event.</param>
    /// <remarks>
    /// This delegate is used instead of <see cref="EventHandler{TEventArgs}"/>
    /// because it allows <typeparamref name="TEventArgs"/> to be a value type,
    /// which reduces the number of runtime allocations.
    /// </remarks>
    public delegate void GenericEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs args);

    /// <summary>
    /// Delegate for a method which can handle generic events without arguments.
    /// </summary>
    /// <typeparam name="TSender">The type of the object that raised the event.</typeparam>
    /// <param name="sender">The object that raised the event.</param>
    public delegate void GenericEventHandler<TSender>(TSender sender);
}
