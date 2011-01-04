using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TKKey = OpenTK.Input.Key;
using TKMouseButton = OpenTK.Input.MouseButton;
using FormsKeys = System.Windows.Forms.Keys;
using FormsMouseButtons = System.Windows.Forms.MouseButtons;
using OrionModifierKeys = Orion.Engine.Gui2.ModifierKeys;

namespace Orion.Engine.Input
{
    /// <summary>
    /// Converts TKKey and mouse button enumerants between representations.
    /// </summary>
    public static class InputEnums
    {
        #region Fields
        private static readonly Dictionary<TKKey, FormsKeys> tkKeyToFormsKeys = new Dictionary<TKKey, FormsKeys>();
        private static readonly Dictionary<FormsKeys, TKKey> formsKeysToTKKey = new Dictionary<FormsKeys, TKKey>();
        #endregion

        #region Constructors
        static InputEnums()
        {
            // The following is taken from OpenTK's source (and reversed)
            AddConversion(TKKey.Escape, FormsKeys.Escape);

            // Function FormsKeys
            for (int i = 0; i < 24; i++) AddConversion(TKKey.F1 + i, (FormsKeys)((int)FormsKeys.F1 + i));

            // Number FormsKeys (0-9)
            for (int i = 0; i <= 9; i++) AddConversion(TKKey.Number0 + i, (FormsKeys)('0' + i));

            // Letters (A-Z)
            for (int i = 0; i < 26; i++) AddConversion(TKKey.A + i, (FormsKeys)('A' + i));

            AddConversion(TKKey.Tab, FormsKeys.Tab);
            AddConversion(TKKey.CapsLock, FormsKeys.Capital);
            AddConversion(TKKey.ControlLeft, FormsKeys.LControlKey);
            AddConversion(TKKey.ShiftLeft, FormsKeys.LShiftKey);
            AddConversion(TKKey.WinLeft, FormsKeys.LWin);
            AddConversion(TKKey.AltLeft, FormsKeys.LMenu);
            AddConversion(TKKey.Space, FormsKeys.Space);
            AddConversion(TKKey.AltRight, FormsKeys.RMenu);
            AddConversion(TKKey.WinRight, FormsKeys.RWin);
            AddConversion(TKKey.Menu, FormsKeys.Apps);
            AddConversion(TKKey.ControlRight, FormsKeys.RControlKey);
            AddConversion(TKKey.ShiftRight, FormsKeys.RShiftKey);
            AddConversion(TKKey.Enter, FormsKeys.Return);
            AddConversion(TKKey.BackSpace, FormsKeys.Back);

            AddConversion(TKKey.Semicolon, FormsKeys.Oem1);      // Varies by keyboard, ;: on Win2K/US
            AddConversion(TKKey.Slash, FormsKeys.Oem2);          // Varies by keyboard, /? on Win2K/US
            AddConversion(TKKey.Tilde, FormsKeys.Oem3);          // Varies by keyboard, `~ on Win2K/US
            AddConversion(TKKey.BracketLeft, FormsKeys.Oem4);    // Varies by keyboard, [{ on Win2K/US
            AddConversion(TKKey.BackSlash, FormsKeys.Oem5);      // Varies by keyboard, \| on Win2K/US
            AddConversion(TKKey.BracketRight, FormsKeys.Oem6);   // Varies by keyboard, ]} on Win2K/US
            AddConversion(TKKey.Quote, FormsKeys.Oem7);          // Varies by keyboard, '" on Win2K/US
            AddConversion(TKKey.Plus, FormsKeys.Oemplus);        // Invariant: +
            AddConversion(TKKey.Comma, FormsKeys.Oemcomma);      // Invariant: ,
            AddConversion(TKKey.Minus, FormsKeys.OemMinus);      // Invariant: -
            AddConversion(TKKey.Period, FormsKeys.OemPeriod);    // Invariant: .

            AddConversion(TKKey.Home, FormsKeys.Home);
            AddConversion(TKKey.End, FormsKeys.End);
            AddConversion(TKKey.Delete, FormsKeys.Delete);
            AddConversion(TKKey.PageUp, FormsKeys.Prior);
            AddConversion(TKKey.PageDown, FormsKeys.Next);
            AddConversion(TKKey.PrintScreen, FormsKeys.Print);
            AddConversion(TKKey.Pause, FormsKeys.Pause);
            AddConversion(TKKey.NumLock, FormsKeys.NumLock);

            AddConversion(TKKey.ScrollLock, FormsKeys.Scroll);
            AddConversion(TKKey.Clear, FormsKeys.Clear);
            AddConversion(TKKey.Insert, FormsKeys.Insert);

            AddConversion(TKKey.Sleep, FormsKeys.Sleep);

            // Keypad
            for (int i = 0; i <= 9; i++)
            {
                AddConversion(TKKey.Keypad0 + i, (FormsKeys)((int)FormsKeys.NumPad0 + i));
            }

            AddConversion(TKKey.KeypadDecimal, FormsKeys.Decimal);
            AddConversion(TKKey.KeypadAdd, FormsKeys.Add);
            AddConversion(TKKey.KeypadSubtract, FormsKeys.Subtract);
            AddConversion(TKKey.KeypadDivide, FormsKeys.Divide);
            AddConversion(TKKey.KeypadMultiply, FormsKeys.Multiply);

            // Navigation
            AddConversion(TKKey.Up, FormsKeys.Up);
            AddConversion(TKKey.Down, FormsKeys.Down);
            AddConversion(TKKey.Left, FormsKeys.Left);
            AddConversion(TKKey.Right, FormsKeys.Right);

            // Side-agnostic modifier FormsKeys
            formsKeysToTKKey.Add(FormsKeys.ShiftKey, TKKey.ShiftLeft);
            formsKeysToTKKey.Add(FormsKeys.ControlKey, TKKey.ControlLeft);
            formsKeysToTKKey.Add(FormsKeys.Menu, TKKey.AltLeft);
        }
        #endregion

        #region Methods
        private static void AddConversion(TKKey tkKey, FormsKeys formsKeys)
        {
            tkKeyToFormsKeys.Add(tkKey, formsKeys);
            formsKeysToTKKey.Add(formsKeys, tkKey);
        }

        public static TKKey GetTKKey(FormsKeys formsKeys)
        {
            TKKey tkKey;
            formsKeysToTKKey.TryGetValue(formsKeys, out tkKey);
            return tkKey;
        }

        public static FormsKeys GetFormsKeys(TKKey tkKey)
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
                case TKMouseButton.Button1: return FormsMouseButtons.XButton1;
                case TKMouseButton.Button2: return FormsMouseButtons.XButton2;
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
