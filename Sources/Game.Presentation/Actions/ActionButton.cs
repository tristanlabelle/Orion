using System;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions
{
	/// <summary>
	/// Describes a button which appears in the action panel of the match UI.
	/// </summary>
    public sealed class ActionButton
    {
        #region Fields
        private string name;
        private string description;
        private Texture texture;
        private Keys hotKey;
        private Action action;
        #endregion
        
        #region Properties
        /// <summary>
        /// Accesses the name of the action represented by this button.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNull(value, "Name");
                name = value;
            }
        }
        
        /// <summary>
        /// Accesses the description of the action represented by this button.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        
        /// <summary>
        /// Accesses the texture displayed on the button.
        /// </summary>
        public Texture Texture
        {
        	get { return texture; }
        	set { texture = value; }
        }
        
        /// <summary>
        /// Accesses the hot key which clicks this button.
        /// </summary>
        public Keys HotKey
        {
        	get { return hotKey; }
        	set { hotKey = value; }
        }
        
        /// <summary>
        /// Accesses the action resulting the clicking of this button.
        /// </summary>
        public Action Action
        {
        	get { return action; }
        	set { action = value; }
        }
        
        /// <summary>
        /// Gets the tool tip text which appears for this button.
        /// </summary>
        public string TooltipText
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(name);

                if (HotKey != Keys.None)
                {
                    stringBuilder.Append(" (");
                    stringBuilder.Append(HotKey.ToStringInvariant());
                    stringBuilder.Append(')');
                }

                if (description != null)
                {
                    stringBuilder.Append('\n');
                    stringBuilder.Append(description);
                }

                return stringBuilder.ToString();
            }
        }
        #endregion
    }
}
