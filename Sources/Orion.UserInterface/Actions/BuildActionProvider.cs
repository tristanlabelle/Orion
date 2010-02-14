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
        private readonly ActionButton[,] buttons = new ActionButton[4, 4];
        #endregion

        #region Constructors
        public BuildActionProvider(ActionFrame actionFrame, UserInputManager inputManager, UnitType unitType,
            UnitTypeRegistry unitTypeRegistry, TextureManager textureManager)
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

            buttons[3, 0] = new CancelButton(actionFrame, inputManager, textureManager);
        }
        #endregion

        #region Methods
        public ActionButton GetButtonAt(int x, int y)
        {
            return buttons[x, y];
        }
        #endregion
    }
}
