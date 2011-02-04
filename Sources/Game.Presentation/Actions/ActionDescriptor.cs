using System;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Key = OpenTK.Input.Key;

namespace Orion.Game.Presentation.Actions
{
	/// <summary>
	/// Describes a button which appears in the action panel of the match UI.
	/// </summary>
    public sealed class ActionDescriptor
    {
        #region Fields
        private string name;
        private ResourceAmount cost;
        private Texture texture;
        private Key hotKey;
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
        /// Accesses the cost of the action represented by this button.
        /// </summary>
        public ResourceAmount Cost
        {
            get { return cost; }
            set { cost = value; }
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
        public Key HotKey
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
        #endregion

        #region Methods
        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
