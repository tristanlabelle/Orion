using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Commandment;
using Orion.Graphics;

namespace Orion.UserInterface.Actions
{
    public sealed class GenericActionButton : ActionButton
    {
        #region Fields
        private readonly UserInputCommand command;
        #endregion

        #region Constructor
        public GenericActionButton(ActionFrame frame, UserInputManager manager,
            string name, Keys hotkey, UserInputCommand provokedCommand, TextureManager textureManager)
            : base(frame, manager, name, hotkey,textureManager)
        {
            command = provokedCommand;

            Texture texture = textureManager.GetAction(name);
            base.Renderer = new TexturedFrameRenderer(texture);
        }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            base.OnPress();
            inputManager.SelectedCommand = command;
            actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, textureManager));
        }
        #endregion
    }
}
