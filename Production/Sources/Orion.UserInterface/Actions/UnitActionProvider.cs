using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using Orion.UserInterface.Actions.Enablers;
using Orion.Graphics;

namespace Orion.UserInterface.Actions
{
    public sealed class UnitActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionButton[,] actionButtons = new ActionButton[4, 4];
        #endregion

        #region Constructors
        public UnitActionProvider(IEnumerable<ActionEnabler> actionEnablers, UnitType type, UnitsRenderer unitsRenderer)
        {
            Argument.EnsureNotNull(actionEnablers, "actionEnablers");
            Argument.EnsureNotNull(type, "type");
            Argument.EnsureNotNull(unitsRenderer, "unitsRenderer");

            foreach (ActionEnabler enabler in actionEnablers)
                enabler.LetFill(type, actionButtons);
        }
        #endregion

        #region Methods
        public ActionButton GetButtonAt(int x, int y)
        {
            return actionButtons[x, y];
        }
        #endregion
    }
}
