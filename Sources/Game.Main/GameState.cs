using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Audio;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialization, updating and cleanup of a game state.
    /// </summary>
    public abstract class GameState : IDisposable
    {
        #region Fields
        private readonly GameStateManager manager;
        #endregion

        #region Constructors
        protected GameState(GameStateManager manager)
        {
            Argument.EnsureNotNull(manager, "manager");
            this.manager = manager;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if this game state is the currently active one.
        /// </summary>
        public bool IsActive
        {
            get { return manager.ActiveState == this; }
        }

        protected GameStateManager Manager
        {
            get { return manager; }
        }

        /// <summary>
        /// Gets the graphics context which provides a visual representation of the game.
        /// </summary>
        protected GameGraphics Graphics
        {
            get { return manager.Graphics; }
        }

        /// <summary>
        /// Gets the audio context which provides audible representation of game events.
        /// </summary>
        protected GameAudio Audio
        {
            get { return manager.Audio; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Invoked when the game state is first entered.
        /// </summary>
        protected internal virtual void OnEntered() { }

        /// <summary>
        /// Invoked each logic frame to update the game state's logic.
        /// </summary>
        /// <param name="timeDelta">The time elapsed since the last frame.</param>
        protected internal virtual void Update(float timeDeltaInSeconds) { }

        /// <summary>
        /// Invoked each rendering frame to refresh the screen.
        /// </summary>
        /// <param name="graphics">The graphics to be used.</param>
        protected internal virtual void Draw(GameGraphics graphics) { }

        /// <summary>
        /// Invoked when a game state is pushed over this game state.
        /// </summary>
        protected internal virtual void OnShadowed() { }

        /// <summary>
        /// Invoked when a this game state has been made active after
        /// higher game states have been popped.
        /// </summary>
        protected internal virtual void OnUnshadowed() { }

        /// <summary>
        /// Releases all resources used by this game state.
        /// </summary>
        public virtual void Dispose() { }
        #endregion
    }
}
