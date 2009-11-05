using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Commandment;

namespace Orion.UserInterface.Actions
{
    public class GenericActionButton : ActionButton
    {
        #region Fields
        private UserInputCommand command;
        #endregion

        #region Constructor
        public GenericActionButton(ActionFrame frame, UserInputManager manager, string name, Keys hotkey, UserInputCommand provokedCommand)
            : base(frame, manager, name, hotkey)
        {
            command = provokedCommand;
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
