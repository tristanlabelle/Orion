using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Describes a keyboard event.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct KeyEvent
    {
        #region Fields
        private readonly byte type;
        private readonly byte modifierKeys;
        private readonly byte key;
        #endregion

        #region Constructors
        public KeyEvent(KeyEventType type, ModifierKeys modifierKeys, Key key)
        {
            Argument.EnsureNotEqual(key, Key.Unknown, "key");

            this.type = checked((byte)type);
            this.modifierKeys = checked((byte)modifierKeys);
            this.key = checked((byte)key);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of this key event.
        /// </summary>
        public KeyEventType Type
        {
            get { return (KeyEventType)type; }
        }

        /// <summary>
        /// Gets a value indicating if the key is down, whether pressed or repeated.
        /// </summary>
        public bool IsDown
        {
            get { return Type != KeyEventType.Released; }
        }

        /// <summary>
        /// Gets a value indicating if the key is up.
        /// </summary>
        public bool IsUp
        {
            get { return Type == KeyEventType.Released; }
        }

        /// <summary>
        /// Gets the modifier keys that were pressed when the event occured.
        /// </summary>
        public ModifierKeys ModifierKeys
        {
            get { return (ModifierKeys)modifierKeys; }
        }

        /// <summary>
        /// Gets a value indicating if the shift modifier key was down when the event occured.
        /// </summary>
        public bool IsShiftDown
        {
            get { return (ModifierKeys & ModifierKeys.Shift) != 0; }
        }

        /// <summary>
        /// Gets a value indicating if the control modifier key was down when the event occured.
        /// </summary>
        public bool IsControlDown
        {
            get { return (ModifierKeys & ModifierKeys.Control) != 0; }
        }

        /// <summary>
        /// Gets a value indicating if the alt modifier key was down when the event occured.
        /// </summary>
        public bool IsAltDown
        {
            get { return (ModifierKeys & ModifierKeys.Alt) != 0; }
        }

        /// <summary>
        /// Gets the key that was pressed, released or repeated.
        /// </summary>
        public Keys Key
        {
            get { return (Keys)key; }
        }

        /// <summary>
        /// Gets a value indicating if the key that is pressed is any of the two shift keys.
        /// </summary>
        public bool IsAnyShift
        {
            get { return Key == Keys.LeftShift || Key == Keys.RightShift; }
        }

        /// <summary>
        /// Gets a value indicating if the key that is pressed is any of the two control keys.
        /// </summary>
        public bool IsAnyControl
        {
            get { return Key == Keys.LeftControl || Key == Keys.RightControl; }
        }

        /// <summary>
        /// Gets a value indicating if the key that is pressed is any of the two alt keys.
        /// </summary>
        public bool IsAnyAlt
        {
            get { return Key == Keys.LeftAlt || Key == Keys.RightAlt; }
        }

        /// <summary>
        /// Gets a value indicating if the key that is pressed is any of the two super (aka Windows) keys.
        /// </summary>
        public bool IsAnySuper
        {
            get { return Key == Keys.LeftSuper || Key == Keys.RightSuper; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return key + " " + Type.ToStringInvariant().ToLowerInvariant();
        }
        #endregion
    }
}
