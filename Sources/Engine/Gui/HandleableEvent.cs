using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// An aggregation of event handling delegates with a boolean return type that indicates if the event was handled.
    /// This type supports the gui system and isn't intended for other uses.
    /// </summary>
    /// <typeparam name="TOwner">The type of the object owning the event.</typeparam>
    /// <typeparam name="TArg">The type of the event arguments object.</typeparam>
    [Serializable]
    internal struct HandleableEvent<TOwner, TArg> where TOwner : class
    {
        #region Fields
        /// <summary>
        /// The delegates handling this event.
        /// This can be one of:
        /// <list>
        /// <item><c>null</c>, meaning there is no handler.</item>
        /// <item>A <typeparamref name="TDelegate"/> instance, meaning there is a single handler.</item>
        /// <item>A porous array of <typeparamref name="TDelegate"/> where all non-<c>null</c> items are handlers.</item>
        /// </list>
        /// </summary>
        private object handlers;
        #endregion

        #region Methods
        /// <summary>
        /// Adds a handler delegate to this event.
        /// </summary>
        /// <param name="handler">The handler to be added.</param>
        public void AddHandler(Func<TOwner, TArg, bool> handler)
        {
            Argument.EnsureNotNull(handler, "handler");

            if (handlers == null)
            {
                // There was no handler, set the handler as the only one.
                handlers = handler;
                return;
            }

            Func<TOwner, TArg, bool>[] handlerArray = handlers as Func<TOwner, TArg, bool>[];
            if (handlerArray == null)
            {
                // There was already a handler, create an array because we now need multiple handlers.
                handlers = new Func<TOwner, TArg, bool>[4]
                {
                    (Func<TOwner, TArg, bool>)handlers,
                    handler,
                    null,
                    null
                };
                return;
            }

            for (int i = 0; i < handlerArray.Length; ++i)
            {
                if (handlerArray[i] == null)
                {
                    handlerArray[i] = handler;
                    return;
                }
            }

            int index = handlerArray.Length;
            Array.Resize(ref handlerArray, handlerArray.Length * 2);
            handlerArray[index] = handler;

            handlers = handlerArray;
        }

        /// <summary>
        /// Removes a handler from this event.
        /// </summary>
        /// <param name="handler">The handler to be removed.</param>
        /// <returns><c>True</c> if the handler was found and removed, <c>false</c> if it was not found.</returns>
        public bool RemoveHandler(Func<TOwner, TArg, bool> handler)
        {
            if (handler == null) return false;

            Func<TOwner, TArg, bool>[] handlerArray = handlers as Func<TOwner, TArg, bool>[];
            if (handlerArray == null)
            {
                if (ReferenceEquals(handlers, handler))
                {
                    handlers = null;
                    return true;
                }
            }
            else
            {
                for (int i = handlerArray.Length - 1; i >= 0; --i)
                {
                    if (ReferenceEquals(handlerArray[i], handler))
                    {
                        handlerArray[i] = null;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Raises this event.
        /// </summary>
        /// <param name="sender">The object which causes this event to be raised.</param>
        /// <param name="arg">The event argument.</param>
        /// <returns>A value indicating if the event has been handled.</returns>
        public bool Raise(TOwner sender, TArg arg)
        {
            Argument.EnsureNotNull(sender, "sender");

            if (handlers == null) return false;

            var singleHandler = handlers as Func<TOwner, TArg, bool>;
            if (singleHandler != null) return singleHandler(sender, arg);

            bool handled = false;
            foreach (var handler in (Func<TOwner, TArg, bool>[])handlers)
                if (handler != null)
                    handled = handler(sender, arg) || handled;

            return handled;
        }
        #endregion
    }
}
