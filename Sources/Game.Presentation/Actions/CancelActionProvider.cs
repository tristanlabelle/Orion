using System;
using Orion.Engine;

namespace Orion.Game.Presentation.Actions
{
    /// <summary>
    /// A simple <see cref="IActionProvider"/> which provides a single cancel button.
    /// </summary>
    public sealed class CancelActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionDescriptor action;
        #endregion

        #region Constructor
        public CancelActionProvider(ActionPanel actionPanel, UserInputManager inputManager, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            action = actionPanel.CreateCancelAction(inputManager, gameGraphics);
        }
        #endregion

        #region Methods
        public ActionDescriptor GetActionAt(Point point)
        {
            return (point.X == 3 && point.Y == 0) ? action : null;
        }

        void IActionProvider.Refresh() {}
        
        void IDisposable.Dispose() {}
        #endregion
    }
}
