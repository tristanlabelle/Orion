using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TKKeys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using TKMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using FormsKeys = System.Windows.Forms.Keys;
using FormsMouseButtons = System.Windows.Forms.MouseButtons;
using OrionModifierKeys = Orion.Engine.Gui.ModifierKeys;

namespace Orion.Engine.Input
{
    /// <summary>
    /// Converts TKKey and mouse button enumerants between representations.
    /// </summary>
    public static class InputEnums
    {
        #region Fields
        private static readonly Dictionary<TKKeys, FormsKeys> tkKeyToFormsKeys = new Dictionary<TKKeys, FormsKeys>();
        private static readonly Dictionary<FormsKeys, TKKeys> formsKeysToTKKey = new Dictionary<FormsKeys, TKKeys>();
        #endregion

        #region Constructors
        static InputEnums()
        {
            // The following is taken from OpenTK's source (and reversed)
            AddConversion(TKKeys.Escape, FormsKeys.Escape);

            // Function FormsKeys
            for (int i = 0; i < 24; i++) AddConversion(TKKeys.F1 + i, (FormsKeys)((int)FormsKeys.F1 + i));

            // Number FormsKeys (0-9)
            for (int i = 0; i <= 9; i++) AddConversion(TKKeys.D0 + i, (FormsKeys)('0' + i));

            // Letters (A-Z)
            for (int i = 0; i < 26; i++) AddConversion(TKKeys.A + i, (FormsKeys)('A' + i));

            AddConversion(TKKeys.Tab, FormsKeys.Tab);
            AddConversion(TKKeys.CapsLock, FormsKeys.Capital);
            AddConversion(TKKeys.LeftControl, FormsKeys.LControlKey);
            AddConversion(TKKeys.LeftShift, FormsKeys.LShiftKey);
            AddConversion(TKKeys.LeftSuper, FormsKeys.LWin);
            AddConversion(TKKeys.LeftAlt, FormsKeys.LMenu);
            AddConversion(TKKeys.Space, FormsKeys.Space);
            AddConversion(TKKeys.RightAlt, FormsKeys.RMenu);
            AddConversion(TKKeys.RightSuper, FormsKeys.RWin);
            AddConversion(TKKeys.Menu, FormsKeys.Apps);
            AddConversion(TKKeys.RightControl, FormsKeys.RControlKey);
            AddConversion(TKKeys.RightShift, FormsKeys.RShiftKey);
            AddConversion(TKKeys.Enter, FormsKeys.Return);
            AddConversion(TKKeys.Backspace, FormsKeys.Back);

            AddConversion(TKKeys.Semicolon, FormsKeys.Oem1);      // Varies by keyboard, ;: on Win2K/US
            AddConversion(TKKeys.Slash, FormsKeys.Oem2);          // Varies by keyboard, /? on Win2K/US
            AddConversion(TKKeys.GraveAccent, FormsKeys.Oem3);          // Varies by keyboard, `~ on Win2K/US
            AddConversion(TKKeys.LeftBracket, FormsKeys.Oem4);    // Varies by keyboard, [{ on Win2K/US
            AddConversion(TKKeys.Backslash, FormsKeys.Oem5);      // Varies by keyboard, \| on Win2K/US
            AddConversion(TKKeys.RightBracket, FormsKeys.Oem6);   // Varies by keyboard, ]} on Win2K/US
            AddConversion(TKKeys.Apostrophe, FormsKeys.Oem7);          // Varies by keyboard, '" on Win2K/US
            AddConversion(TKKeys.Equal, FormsKeys.Oemplus);        // Invariant: +
            AddConversion(TKKeys.Comma, FormsKeys.Oemcomma);      // Invariant: ,
            AddConversion(TKKeys.Minus, FormsKeys.OemMinus);      // Invariant: -
            AddConversion(TKKeys.Period, FormsKeys.OemPeriod);    // Invariant: .

            AddConversion(TKKeys.Home, FormsKeys.Home);
            AddConversion(TKKeys.End, FormsKeys.End);
            AddConversion(TKKeys.Delete, FormsKeys.Delete);
            AddConversion(TKKeys.PageUp, FormsKeys.Prior);
            AddConversion(TKKeys.PageDown, FormsKeys.Next);
            AddConversion(TKKeys.PrintScreen, FormsKeys.Print);
            AddConversion(TKKeys.Pause, FormsKeys.Pause);
            AddConversion(TKKeys.NumLock, FormsKeys.NumLock);

            AddConversion(TKKeys.ScrollLock, FormsKeys.Scroll);
            //AddConversion(TKKeys.Clear, FormsKeys.Clear);
            AddConversion(TKKeys.Insert, FormsKeys.Insert);

            //AddConversion(TKKeys.Sleep, FormsKeys.Sleep);

            // Keypad
            for (int i = 0; i <= 9; i++)
            {
                AddConversion(TKKeys.KeyPad0 + i, (FormsKeys)((int)FormsKeys.NumPad0 + i));
            }

            AddConversion(TKKeys.KeyPadDecimal, FormsKeys.Decimal);
            AddConversion(TKKeys.KeyPadAdd, FormsKeys.Add);
            AddConversion(TKKeys.KeyPadSubtract, FormsKeys.Subtract);
            AddConversion(TKKeys.KeyPadDivide, FormsKeys.Divide);
            AddConversion(TKKeys.KeyPadMultiply, FormsKeys.Multiply);

            // Navigation
            AddConversion(TKKeys.Up, FormsKeys.Up);
            AddConversion(TKKeys.Down, FormsKeys.Down);
            AddConversion(TKKeys.Left, FormsKeys.Left);
            AddConversion(TKKeys.Right, FormsKeys.Right);

            // Side-agnostic modifier FormsKeys
            formsKeysToTKKey.Add(FormsKeys.ShiftKey, TKKeys.LeftShift);
            formsKeysToTKKey.Add(FormsKeys.ControlKey, TKKeys.LeftControl);
            formsKeysToTKKey.Add(FormsKeys.Menu, TKKeys.LeftAlt);
        }
        #endregion

        #region Methods
        private static void AddConversion(TKKeys tkKey, FormsKeys formsKeys)
        {
            tkKeyToFormsKeys.Add(tkKey, formsKeys);
            formsKeysToTKKey.Add(formsKeys, tkKey);
        }

        public static TKKeys GetTKKey(FormsKeys formsKeys)
        {
            TKKeys tkKey;
            formsKeysToTKKey.TryGetValue(formsKeys, out tkKey);
            return tkKey;
        }

        public static FormsKeys GetFormsKeys(TKKeys tkKey)
        {
            FormsKeys formsKeys;
            tkKeyToFormsKeys.TryGetValue(tkKey, out formsKeys);
            return formsKeys;
        }

        public static FormsMouseButtons GetFormsMouseButtons(TKMouseButton tkMouseButton)
        {
            switch (tkMouseButton)
            {
                case TKMouseButton.Left: return FormsMouseButtons.Left;
                case TKMouseButton.Middle: return FormsMouseButtons.Middle;
                case TKMouseButton.Right: return FormsMouseButtons.Right;
                case TKMouseButton.Button4: return FormsMouseButtons.XButton1;
                case TKMouseButton.Button5: return FormsMouseButtons.XButton2;
                default: return FormsMouseButtons.None;
            }
        }

        public static TKMouseButton? GetTKMouseButton(FormsMouseButtons formsMouseButton)
        {
            switch (formsMouseButton)
            {
                case FormsMouseButtons.Left: return TKMouseButton.Left;
                case FormsMouseButtons.Middle: return TKMouseButton.Middle;
                case FormsMouseButtons.Right: return TKMouseButton.Right;
                case FormsMouseButtons.XButton1: return TKMouseButton.Button1;
                case FormsMouseButtons.XButton2: return TKMouseButton.Button2;
                default: return null;
            }
        }

        public static OrionModifierKeys GetOrionModifierKeys(FormsKeys formsKeys)
        {
            formsKeys &= FormsKeys.Modifiers;
            OrionModifierKeys orionModifierKeys = OrionModifierKeys.None;
            if ((formsKeys & FormsKeys.Shift) != 0) orionModifierKeys |= OrionModifierKeys.Shift;
            if ((formsKeys & FormsKeys.Control) != 0) orionModifierKeys |= OrionModifierKeys.Control;
            if ((formsKeys & FormsKeys.Alt) != 0) orionModifierKeys |= OrionModifierKeys.Alt;
            return orionModifierKeys;
        }

        public static FormsKeys GetFormsModifierKeys(OrionModifierKeys orionModifierKeys)
        {
            FormsKeys formsKeys = FormsKeys.None;
            if ((orionModifierKeys & OrionModifierKeys.Shift) != 0) formsKeys |= FormsKeys.Shift;
            if ((orionModifierKeys & OrionModifierKeys.Control) != 0) formsKeys |= FormsKeys.Control;
            if ((orionModifierKeys & OrionModifierKeys.Alt) != 0) formsKeys |= FormsKeys.Alt;
            return formsKeys;
        }
        #endregion
    }
}
