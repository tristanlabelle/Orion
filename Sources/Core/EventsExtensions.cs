using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    /// <summary>
    /// Provides extension methods for delegates used as events.
    /// </summary>
    public static class EventsExtensions
    {
        /// <summary>
        /// Raises an event, if it isn't null.
        /// </summary>
        /// <param name="event">The event to be raised.</param>
        public static void Raise(this Action @event)
        {
            if (@event != null) @event();
        }

        /// <summary>
        /// Raises an event, if it isn't null.
        /// </summary>
        /// <typeparam name="T">The type of the event argument.</typeparam>
        /// <param name="event">The event to be raised.</param>
        /// <param name="arg">The argument of the event.</param>
        public static void Raise<T>(this Action<T> @event, T arg)
        {
            if (@event != null) @event(arg);
        }

        /// <summary>
        /// Raises an event, if it isn't null.
        /// </summary>
        /// <typeparam name="T1">The type of the first event argument.</typeparam>
        /// <typeparam name="T2">The type of the second event argument.</typeparam>
        /// <param name="event">The event to be raised.</param>
        /// <param name="arg1">The first argument of the event.</param>
        /// <param name="arg2">The second argument of the event.</param>
        public static void Raise<T1, T2>(this Action<T1, T2> @event, T1 arg1, T2 arg2)
        {
            if (@event != null) @event(arg1, arg2);
        }
    }
}
