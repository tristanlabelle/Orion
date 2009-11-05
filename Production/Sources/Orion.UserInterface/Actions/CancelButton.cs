using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Commandment;

namespace Orion.UserInterface.Actions
{
    public class CancelButton : ActionButton
    {
        public CancelButton(ActionFrame frame, UserInputManager manager)
            : base(frame, manager, "Cancel", Keys.Escape)
        { }

        protected override void OnPress()
        {
            inputManager.SelectedCommand = null;
            container.Restore();
        }
    }
}
