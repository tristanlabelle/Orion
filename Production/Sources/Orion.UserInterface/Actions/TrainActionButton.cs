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
    public class TrainActionButton : ActionButton
    {
        #region Fields
        private ImmediateUserCommand command;
        #endregion

        #region Constructors
        public TrainActionButton(ActionFrame frame, UserInputManager manager,
            string name, Keys hotkey, ImmediateUserCommand command, TextureManager textureManager)
            : base(frame, manager, name, hotkey, textureManager)
        {
            this.command = command;

            Texture texture = textureManager.GetUnit(name);
            base.Renderer = new TexturedFrameRenderer(texture);
        }

        public TrainActionButton(ActionFrame frame, UserInputManager manager,
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
