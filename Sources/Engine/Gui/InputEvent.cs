using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OpenTK.Math;
using System.Runtime.InteropServices;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Represents a keyboard or mouse event that occured on the window.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    [StructLayout(LayoutKind.Explicit)]
    public struct InputEvent
    {
        #region Instance
        #region Fields
        [FieldOffset(0)]
        private readonly InputEventType type;
        [FieldOffset(4)]
        private readonly int subtype;
        [FieldOffset(8)]
        private readonly KeyboardEventArgs keyboardArgs;
        [FieldOffset(8)]
        private readonly MouseEventArgs mouseArgs;
        [FieldOffset(8)]
        private readonly char character;
        #endregion

        #region Constructors
        private InputEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            this.type = InputEventType.Keyboard;
            this.subtype = (int)type;

            this.character = default(char);
            this.mouseArgs = default(MouseEventArgs);

            this.keyboardArgs = args;
        }

        private InputEvent(MouseEventType type, MouseEventArgs args)
        {
            this.type = InputEventType.Mouse;
            this.subtype = (int)type;

            this.character = default(char);
            this.keyboardArgs = default(KeyboardEventArgs);

            this.mouseArgs = args;
        }

        private InputEvent(char character)
        {
            this.type = InputEventType.Character;
            this.subtype = 0;

            this.mouseArgs = default(MouseEventArgs);
            this.keyboardArgs = default(KeyboardEventArgs);

            this.character = default(char);
        }
        #endregion

        #region Properties
        public InputEventType Type
        {
            get { return type; }
        }
        #endregion

        #region Methods
        public void GetKeyboard(out KeyboardEventType type, out KeyboardEventArgs args)
        {
            EnsureValidType(InputEventType.Keyboard);
            type = (KeyboardEventType)this.subtype;
            args = keyboardArgs;
        }

        public void GetMouse(out MouseEventType type, out MouseEventArgs args)
        {
            EnsureValidType(InputEventType.Mouse);
            type = (MouseEventType)this.subtype;
            args = mouseArgs;
        }

        public void GetCharacter(out char character)
        {
            EnsureValidType(InputEventType.Character);
            character = this.character;
        }

        private void EnsureValidType(InputEventType type)
        {
            if (type != this.type)
            {
                throw new InvalidOperationException(
                    "Cannot read a {0} event as a {1} event."
                    .FormatInvariant(this.type, type));
            }
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static InputEvent CreateKeyboard(KeyboardEventType type, KeyboardEventArgs args)
        {
            return new InputEvent(type, args);
        }

        public static InputEvent CreateMouse(MouseEventType type, MouseEventArgs args)
        {
            return new InputEvent(type, args);
        }

        public static InputEvent CreateCharacter(char character)
        {
            return new InputEvent(character);
        }
        #endregion
        #endregion
    }
}
