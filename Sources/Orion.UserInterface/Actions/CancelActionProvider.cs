﻿using System;
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

            button = new CancelButton(actionFrame, inputManager, textureManager); //, "Cancel", Keys.Escape, textureManager);

            //Texture texture = textureManager.GetAction("Cancel");
            //button.Renderer = new TexturedFrameRenderer(texture);

            //button.Triggered += delegate(Button sender)
            //{
            //    inputManager.SelectedCommand = null;
            //    actionFrame.Restore();
            //};
        }
        #endregion

        #region Methods
        public ActionButton GetButtonAt(int x, int y)
        {
            return (x == 3 && y == 0) ? button : null;
        }
        #endregion
    }
}
