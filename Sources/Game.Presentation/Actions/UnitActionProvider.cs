using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.Enablers;

namespace Orion.Game.Presentation.Actions
{
    public sealed class UnitActionProvider : IActionProvider
    {
        #region Fields
        private readonly List<ActionEnabler> enablers;
        private readonly UnitType unitType;
        private readonly ActionButton[,] buttons = new ActionButton[4, 4];
        #endregion

        #region Constructors
        public UnitActionProvider(IEnumerable<ActionEnabler> actionEnablers, UnitType unitType)
        {
            Argument.EnsureNotNull(actionEnablers, "actionEnablers");
            Argument.EnsureNotNull(unitType, "unitType");

            this.enablers = actionEnablers.ToList();
            Argument.EnsureNoneNull(this.enablers, "actionEnablers");
            this.unitType = unitType;

            CreateButtons();
        }
        #endregion

        #region Methods
        public ActionButton GetButtonAt(Point point)
        {
            return buttons[point.X, point.Y];
        }

        public void Refresh()
        {
            ClearButtons();
            CreateButtons();
        }

        public void Dispose()
        {
            ClearButtons();
        }

        private void CreateButtons()
        {
            foreach (ActionEnabler enabler in enablers)
                enabler.LetFill(unitType, buttons);
        }

        private void ClearButtons()
        {
            for (int y = 0; y < buttons.GetLength(1); ++y)
                for (int x = 0; x < buttons.GetLength(0); ++x)
                        buttons[x, y] = null;
        }
        #endregion
    }
}
