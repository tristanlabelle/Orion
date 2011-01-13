using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Presentation;
using Orion.Game.Matchmaking;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions
{
    /// <summary>
    /// A simple <see cref="IActionProvider"/> which provides a single cancel button.
    /// </summary>
    public sealed class CancelActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionButton button;
        #endregion

        #region Constructor
        public CancelActionProvider(ActionPanel actionPanel, UserInputManager inputManager, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            button = actionPanel.CreateCancelButton(inputManager, gameGraphics);
        }
        #endregion

        #region Methods
        public ActionButton GetButtonAt(Point point)
        {
            return (point.X == 3 && point.Y == 0) ? button : null;
        }

        void IActionProvider.Refresh() {}
        
        void IDisposable.Dispose() {}
        #endregion
    }
}
