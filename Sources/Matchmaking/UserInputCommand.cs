using System;
using OpenTK.Math;
using Orion.GameLogic;

namespace Orion.Matchmaking
{
    /// <summary>
    /// Represents a tool which handles the left click to
    /// trigger a user command.
    /// </summary>
    public abstract class UserInputCommand
    {
        #region Fields
        private readonly UICommander uiCommander;
        #endregion

        #region Constructors
        protected UserInputCommand(UICommander uiCommander)
        {
            Argument.EnsureNotNull(uiCommander, "uiCommander");
            this.uiCommander = uiCommander;
        }
        #endregion

        #region Properties
        protected UICommander InputManager
        {
            get { return uiCommander; }
        }

        protected World World
        {
            get { return uiCommander.World; }
        }

        protected Faction LocalFaction
        {
            get { return uiCommander.Faction; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Invoked when the mouse moves.
        /// </summary>
        /// <param name="location">The new location of the cursor, in world coordinates.</param>
        public virtual void OnMouseMoved(Vector2 location) { }

        /// <summary>
        /// Invoked when the left mouse button is clicked.
        /// </summary>
        /// <param name="location">The location, in world coordinates, where the click occured.</param>
        /// <remarks>
        /// This is the place to cause an action as the command is discarded after the call.
        /// </remarks>
        public abstract void OnClick(Vector2 location);
        #endregion
    }
}
