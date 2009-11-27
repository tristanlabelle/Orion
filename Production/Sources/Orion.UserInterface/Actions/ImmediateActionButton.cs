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
            string name, Keys hotkey, ImmediateUserCommand command, Texture texture)
            : base(frame, manager, name, hotkey)
        {
            this.command = command;
            if (texture != null) base.Renderer = new TexturedFrameRenderer(texture);
        }

        public ImmediateActionButton(ActionFrame frame, UserInputManager manager,
            Keys hotkey, ImmediateUserCommand command, Texture texture)
            : this(frame, manager, "", hotkey, command, texture)
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
