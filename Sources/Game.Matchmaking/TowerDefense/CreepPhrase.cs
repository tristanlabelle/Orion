using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.Game.Matchmaking.TowerDefense
{
    public sealed class CreepPhrase
    {
        #region Fields
        private readonly string text;
        private int typedCharacterCount;
        private bool isFocused;
        #endregion

        #region Constructors
        public CreepPhrase(string text)
        {
            Debug.Assert(text != null);
            this.text = text;
        }
        #endregion

        #region Properties
        public string Text
        {
            get { return text; }
        }

        public int TypedCharacterCount
        {
            get { return typedCharacterCount; }
        }
        
        public bool IsComplete
        {
            get { return typedCharacterCount == text.Length; }
        }

        public char NextCharacter
        {
            get { return IsComplete ? '\0' : text[typedCharacterCount]; }
        }

        public bool IsFocused
        {
            get { return isFocused; }
        }
        #endregion

        #region Methods
        public bool Type(char character)
        {
            if (IsComplete || character != text[typedCharacterCount]) return false;
            ++typedCharacterCount;
            return true;
        }

        public void Focus()
        {
            isFocused = true;
        }

        public void Unfocus()
        {
            isFocused = false;
            typedCharacterCount = 0;
        }

        public override string ToString()
        {
            return text;
        }
        #endregion
    }
}
