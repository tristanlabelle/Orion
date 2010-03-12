﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Graphics;
using Orion.Matchmaking;
using Orion.UserInterface.Widgets;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions
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
        public CancelActionProvider(ActionFrame actionFrame, UICommander uiCommander, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(uiCommander, "uiCommander");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            button = actionFrame.CreateCancelButton(uiCommander, gameGraphics);
        }
        #endregion

        #region Methods
        public ActionButton GetButtonAt(Point point)
        {
            return (point.X == 3 && point.Y == 0) ? button : null;
        }

        public void Dispose()
        {
            button.Dispose();
        }

        void IActionProvider.Refresh() {}
        #endregion
    }
}
