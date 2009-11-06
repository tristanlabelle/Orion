using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using Orion.UserInterface.Actions.Enablers;

namespace Orion.UserInterface.Actions
{
    public class UnitActionProvider : IActionProvider
    {
        private readonly ActionButton[,] actionButtons = new ActionButton[4, 4];

        public UnitActionProvider(IEnumerable<ActionEnabler> actionEnablers, UnitType type)
        {
            foreach (ActionEnabler enabler in actionEnablers) enabler.LetFill(type, actionButtons);
        }

        public ActionButton GetButtonAt(int x, int y)
        {
            return actionButtons[x, y];
        }
    }
}
