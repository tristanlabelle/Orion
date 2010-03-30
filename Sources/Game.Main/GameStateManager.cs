using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;

namespace Orion.Game.Main
{
    /// <summary>
    /// Maintains a stack of game states and manages the transitions between them.
    /// </summary>
    public sealed class GameStateManager
    {
        #region Fields
        private readonly Stack<GameState> states = new Stack<GameState>();
        private bool deferActions;
        private Action deferredAction;
        #endregion

        #region Constructors
        public GameStateManager() { }
        #endregion

        #region Properties
        public GameState ActiveState
        {
            get { return states.Count == 0 ? null : states.Peek(); }
        }

        /// <summary>
        /// Gets a value indicating if there currently is no game state pushed on the stack.
        /// </summary>
        public bool IsEmpty
        {
            get { return states.Count == 0; }
        }
        #endregion

        #region Methods
        #region Pushing/Popping
        /// <summary>
        /// Pushes a game state of top of the game state stack.
        /// </summary>
        /// <param name="state">The game state to be pushed. Its ownership is transferred to this manager.</param>
        public void Push(GameState state)
        {
            Argument.EnsureNotNull(state, "state");

            if (deferActions)
            {
                Debug.Assert(deferredAction == null);
                deferredAction = () => CommitPush(state);
                return;
            }

            CommitPush(state);
        }

        private void CommitPush(GameState state)
        {
            if (states.Count > 0)
            {
                GameState previouslyActive = states.Peek();
                previouslyActive.OnShadowed();
            }

            states.Push(state);
            state.OnEntered();
        }

        public void Pop()
        {
            if (states.Count == 0) return;

            if (deferActions)
            {
                Debug.Assert(deferredAction == null);
                deferredAction = () => CommitPop();
                return;
            }

            CommitPop();
        }

        private void CommitPop()
        {
            Debug.Assert(states.Count > 0);

            GameState previouslyActive = states.Pop();
            previouslyActive.Dispose();

            if (states.Count > 0)
            {
                GameState newActive = states.Peek();
                newActive.OnUnshadowed();
            }
        }

        public void PopTo<TGameState>() where TGameState : GameState
        {
            if (deferActions)
            {
                Debug.Assert(deferredAction == null);
                deferredAction = () => CommitPopTo<TGameState>();
                return;
            }

            CommitPopTo<TGameState>();
        }

        private void CommitPopTo<TGameState>()
        {
            GameState previouslyActive = states.Pop();
            previouslyActive.Dispose();

            while (true)
            {
                if (states.Count == 0) break;

                GameState topmost = states.Peek();
                if (topmost is TGameState)
                {
                    topmost.OnUnshadowed();
                    break;
                }

                states.Pop();
                topmost.Dispose();
            }
        }

        private void ExecuteDeferredActions()
        {
            if (deferredAction != null)
            {
                deferredAction();
                deferredAction = null;
            }
        }
        #endregion

        public void Update(float timeDelta)
        {
            if (IsEmpty) return;

            GameState activeState = states.Peek();

            try
            {
                deferActions = true;
                activeState.Update(timeDelta);
            }
            finally
            {
                deferActions = false;
            }

            ExecuteDeferredActions();
        }

        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            if (IsEmpty) return;

            GameState activeState = states.Peek();

            try
            {
                deferActions = true;
                activeState.Draw(graphics);
            }
            finally
            {
                deferActions = false;
            }

            ExecuteDeferredActions();
        }
        #endregion
    }
}
