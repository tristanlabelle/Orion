using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using Orion.UserInterface.Actions.Enablers;

namespace Orion.UserInterface.Actions
{
    public class UnitTypeActionProvider : IActionProvider
    {
        private readonly ActionButton[,] actionButtons = new ActionButton[4, 4];

        public UnitTypeActionProvider(IEnumerable<ActionEnabler> actionEnablers, UnitType type)
        {
            foreach (ActionEnabler enabler in actionEnablers) enabler.LetFill(type, actionButtons);
        }

        public ActionButton this[int x, int y]
        {
            get { return actionButtons[x, y]; }
        }
    }
}
