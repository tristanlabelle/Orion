using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Orion.Engine.Gui2
{
    internal struct HandleableEvent<TDelegate> where TDelegate : class
    {
        #region Fields
        private object handlers;
        #endregion

        #region Properties
        internal object Handlers
        {
            get { return handlers; }
        }
        #endregion

        #region Methods
        public void AddHandler(TDelegate handler)
        {
            Argument.EnsureNotNull(handler, "handler");

            if (handlers == null)
            {
                // There was no handler, set the handler as the only one.
                handlers = handler;
                return;
            }

            TDelegate[] handlerArray = handlers as TDelegate[];
            if (handlerArray == null)
            {
                // There was already a handler, create an array because we now need multiple handlers.
                handlers = new TDelegate[4]
                {
                    (TDelegate)handlers,
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

        public bool RemoveHandler(TDelegate handler)
        {
            Argument.EnsureNotNull(handler, "handler");

            TDelegate[] handlerArray = handlers as TDelegate[];
            if (handlerArray == null)
            {
                if (handlers == handler)
                {
                    handlers = null;
                    return true;
                }
            }
            else
            {
                for (int i = 0; i < handlerArray.Length; ++i)
                {
                    if (handlerArray[i] == handler)
                    {
                        handlerArray[i] = null;
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
    }

    internal static class HandleableEvent
    {
        public static bool Raise<TArg1, TArg2>(this HandleableEvent<Func<TArg1, TArg2, bool>> @event, TArg1 arg1, TArg2 arg2)
        {
            if (@event.Handlers == null) return false;

            var handler = @event.Handlers as Func<TArg1, TArg2, bool>;
            if (handler != null) return handler(arg1, arg2);

            bool handled = false;
            foreach (var handler2 in (Func<TArg1, TArg2, bool>[])@event.Handlers)
                if (handler2 != null)
                    handled = handler2(arg1, arg2) || handled;

            return handled;
        }

        public static bool Raise<TArg1, TArg2, TArg3>(this HandleableEvent<Func<TArg1, TArg2, TArg3, bool>> @event, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            if (@event.Handlers == null) return false;

            var handler = @event.Handlers as Func<TArg1, TArg2, TArg3, bool>;
            if (handler != null) return handler(arg1, arg2, arg3);

            bool handled = false;
            foreach (var handler2 in (Func<TArg1, TArg2, TArg3, bool>[])@event.Handlers)
                if (handler2 != null)
                    handled = handler2(arg1, arg2, arg3) || handled;

            return handled;
        }

        public static bool Raise<TArg1, TArg2, TArg3, TArg4>(this HandleableEvent<Func<TArg1, TArg2, TArg3, TArg4, bool>> @event, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            if (@event.Handlers == null) return false;

            var handler = @event.Handlers as Func<TArg1, TArg2, TArg3, TArg4, bool>;
            if (handler != null) return handler(arg1, arg2, arg3, arg4);

            bool handled = false;
            foreach (var handler2 in (Func<TArg1, TArg2, TArg3, TArg4, bool>[])@event.Handlers)
                if (handler2 != null)
                    handled = handler2(arg1, arg2, arg3, arg4) || handled;

            return handled;
        }
    }
}
