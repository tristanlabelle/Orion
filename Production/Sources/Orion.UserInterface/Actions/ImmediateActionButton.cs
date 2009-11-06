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
        private ImmediateUserCommand command;

        public ImmediateActionButton(ActionFrame frame, UserInputManager manager, string name, Keys hotkey, ImmediateUserCommand command)
            : base(frame, manager, name, hotkey)
        {
            this.command = command;
        }

        protected override void OnPress()
        {
            command.Execute();
        }
    }
}
