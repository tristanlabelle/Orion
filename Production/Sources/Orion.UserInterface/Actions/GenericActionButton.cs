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
            string name, Keys hotkey, UserInputCommand provokedCommand, Texture texture)
            : base(frame, manager, name, hotkey)
        {
            command = provokedCommand;
            if (texture != null) base.Renderer = new TexturedFrameRenderer(texture);
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
