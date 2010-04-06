using System;
using Keys = System.Windows.Forms.Keys;
using System.ComponentModel;
using System.Diagnostics;

namespace Orion.Engine.Input
{
    /// <summary>
    /// This immutable structure encapsulates data about a keyboard event.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    [DebuggerDisplay("Key={Key}, Modifiers={Modifiers}")]
    public struct KeyboardEventArgs
    {
        #region Fields
        private readonly Keys keyAndModifiers;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a <see cref="KeyboardEventArgs"/>.
        /// </summary>
        /// <param name="keyAndModifiers">The pressed virtual key and its modifiers.</param>
        public KeyboardEventArgs(Keys keyAndModifiers)
        {
            this.keyAndModifiers = keyAndModifiers;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the key involved in this event with its modifiers or'd.
        /// </summary>
        public Keys KeyAndModifiers
        {
            get { return keyAndModifiers; }
        }

        /// <summary>
        /// Gets the key involved in this event.
        /// </summary>
        public Keys Key
        {
            get { return keyAndModifiers & ~Keys.Modifiers; }
        }

        /// <summary>
        /// Gets the modifiers which are pressed.
        /// </summary>
        public Keys Modifiers
        {
            get { return keyAndModifiers & Keys.Modifiers; }
        }

        /// <summary>
        /// Gets a value indicating if the shift modifier is currently down.
        /// </summary>
        public bool IsShiftModifierDown
        {
            get { return (keyAndModifiers & Keys.Shift) != 0; }
        }

        /// <summary>
        /// Gets a value indicating if the control modifier is currently down.
        /// </summary>
        public bool IsControlModifierDown
        {
            get { return (keyAndModifiers & Keys.Control) != 0; }
        }

        /// <summary>
        /// Gets a value indicating if the alt modifier is currently down.
        /// </summary>
        public bool IsAltModifierDown
        {
            get { return (keyAndModifiers & Keys.Alt) != 0; }
        }
        #endregion
    }
}
