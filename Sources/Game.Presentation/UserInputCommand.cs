using System;
using OpenTK.Math;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Represents a tool which handles the left click to
    /// trigger a user command.
    /// </summary>
    public abstract class UserInputCommand
    {
        #region Fields
        private readonly UserInputManager inputManager;
        #endregion

        #region Constructors
        protected UserInputCommand(UserInputManager inputManager)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            this.inputManager = inputManager;
        }
        #endregion

        #region Properties
        protected UserInputManager InputManager
        {
            get { return inputManager; }
        }

        protected Match Match
        {
            get { return inputManager.Match; }
        }

        protected World World
        {
            get { return inputManager.World; }
        }

        protected Faction LocalFaction
        {
            get { return inputManager.LocalFaction; }
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
