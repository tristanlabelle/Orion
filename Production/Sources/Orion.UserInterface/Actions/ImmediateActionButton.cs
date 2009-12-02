using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Graphics;
using Orion.Geometry;
using Orion.Commandment;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface.Actions
{
    public class ImmediateActionButton : ActionButton
    {
        #region Fields
        private ImmediateUserCommand command;
        #endregion

        #region Constructors
        public ImmediateActionButton(ActionFrame frame, UserInputManager manager,
            string name, Keys hotkey, ImmediateUserCommand command, TextureManager textureManager)
            : base(frame, manager, name, hotkey, textureManager)
        {
            this.command = command;
            base.Renderer = new TexturedFrameRenderer(textureManager.GetTexture("name"));
        }

        public ImmediateActionButton(ActionFrame frame, UserInputManager manager,
            Keys hotkey, ImmediateUserCommand command, TextureManager textureManager)
            : this(frame, manager, "", hotkey, command, textureManager)
        { }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            command.Execute();
        }
        #endregion
    }
}
