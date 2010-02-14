using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Commandment;
using Orion.Graphics;
using Keys = System.Windows.Forms.Keys;
using Orion.UserInterface.Widgets;

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
        public CancelActionProvider(ActionFrame actionFrame, UserInputManager inputManager, TextureManager textureManager)
        {
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(textureManager, "textureManager");

            button = actionFrame.CreateCancelButton(inputManager, textureManager);
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
