﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions
{
    /// <summary>
    /// Provides the buttons to build each <see cref="UnitType"/> that is supported by a builder unit.
    /// </summary>
    public sealed class UpgradeActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionPanel actionPanel;
        private readonly UserInputManager inputManager;
        private readonly GameGraphics graphics;
        private readonly UnitType unitType;
        private readonly ActionDescriptor[,] buttons = new ActionDescriptor[4, 4];
        #endregion

        #region Constructors
        public UpgradeActionProvider(ActionPanel actionPanel, UserInputManager inputManager,
            GameGraphics graphics, UnitType unitType)
        {
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(unitType, "unitType");

            this.actionPanel = actionPanel;
            this.inputManager = inputManager;
            this.graphics = graphics;
            this.unitType = unitType;

            CreateButtons();
        }
        #endregion

        #region Methods
        public ActionDescriptor GetActionAt(Point point)
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
            int x = 0;
            int y = 3;

            foreach (UnitTypeUpgrade upgrade in unitType.Upgrades.Where(u => !u.IsFree))
            {
                UnitType targetType = inputManager.Match.UnitTypes.FromName(upgrade.Target);
                if (targetType == null) continue;

                buttons[x, y] = new ActionDescriptor()
	            {
	            	Name = upgrade.Target,
	            	Description = "Aladdium: {1} / Alagene: {2}".FormatInvariant(upgrade.AladdiumCost, upgrade.AlageneCost),
	            	Texture = graphics.GetUnitTexture(targetType),
	            	Action = () => inputManager.LaunchUpgrade(targetType)
	            };

                x++;
                if (x == 4)
                {
                    y--;
                    x = 0;
                }
            }

            buttons[3, 0] = actionPanel.CreateCancelAction(inputManager, graphics);
        }

        private void ClearButtons()
        {
            for (int y = 0; y < buttons.GetLength(1); ++y)
                for (int x = 0; x < buttons.GetLength(0); ++x)
                    if (buttons[x, y] != null)
                        buttons[x, y] = null;
        }
        #endregion
    }
}
