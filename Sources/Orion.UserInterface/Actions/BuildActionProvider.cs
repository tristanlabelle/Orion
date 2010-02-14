using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.GameLogic.Skills;
using System.Diagnostics;

namespace Orion.UserInterface.Actions
{
    /// <summary>
    /// Provides the buttons to build each <see cref="UnitType"/> that is supported by a builder unit.
    /// </summary>
    public sealed class BuildActionProvider : IActionProvider
    {
        #region Fields
        private readonly ActionFrame actionFrame;
        private readonly UserInputManager inputManager;
        private readonly UnitType unitType;
        private readonly UnitTypeRegistry unitTypeRegistry;
        private readonly TextureManager textureManager;
        private readonly ActionButton[,] buttons = new ActionButton[4, 4];
        #endregion

        #region Constructors
        public BuildActionProvider(ActionFrame actionFrame, UserInputManager inputManager, UnitType unitType,
            UnitTypeRegistry unitTypeRegistry, TextureManager textureManager)
        {
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(unitType, "unitType");
            Argument.EnsureNotNull(unitTypeRegistry, "unitTypeRegistry");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.actionFrame = actionFrame;
            this.inputManager = inputManager;
            this.unitType = unitType;
            this.unitTypeRegistry = unitTypeRegistry;
            this.textureManager = textureManager;

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
            DisposeButtons();
            CreateButtons();
        }

        public void Dispose()
        {
            DisposeButtons();
        }

        private void CreateButtons()
        {
            BuildSkill buildSkill = unitType.GetSkill<BuildSkill>();
            Debug.Assert(buildSkill != null);

            int x = 0;
            int y = 3;
            foreach (UnitType buildingType in unitTypeRegistry.Where(u => buildSkill.Supports(u)))
            {
                buttons[x, y] = new BuildingConstructionActionButton(actionFrame, inputManager,
                    buildingType, inputManager.LocalCommander.Faction, textureManager);

                x++;
                if (x == 4)
                {
                    y--;
                    x = 0;
                }
            }

            buttons[3, 0] = actionFrame.CreateCancelButton(inputManager, textureManager);
        }

        private void DisposeButtons()
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
