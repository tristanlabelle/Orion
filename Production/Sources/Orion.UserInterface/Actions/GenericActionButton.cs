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
            base.Renderer = new TexturedFrameRenderer(textureManager.GetTexture(name));
        }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            inputManager.SelectedCommand = command;
            base.OnPress();
        }
        #endregion
    }
}
