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
        private readonly ActionButton[,] buttons = new ActionButton[4, 4];
        #endregion

        #region Constructors
        public UnitActionProvider(IEnumerable<ActionEnabler> actionEnablers, UnitType type)
        {
            Argument.EnsureNotNull(actionEnablers, "actionEnablers");
            Argument.EnsureNotNull(type, "type");

            foreach (ActionEnabler enabler in actionEnablers)
                enabler.LetFill(type, buttons);
        }
        #endregion

        #region Methods
        public ActionButton GetButtonAt(Point point)
        {
            return buttons[point.X, point.Y];
        }

        public void Dispose()
        {
            for (int y = 0; y < buttons.GetLength(1); ++y)
            {
                for (int x = 0; x < buttons.GetLength(0); ++x)
                {
                    if (buttons[x, y] != null)
                    {
                        buttons[x, y].Dispose();
                        buttons[x, y] = null;
                    }
                }
            }
        }
        #endregion
    }
}
