using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Skills = Orion.GameLogic.Skills;
using Orion.Commandment;
using Orion.GameLogic;
using OpenTK.Math;
using Orion.Graphics;

namespace Orion.UserInterface.Actions
{
    public class BuildActionButton : ActionButton
    {
        #region Fields
        private ActionButton[,] buildingButtons = new ActionButton[4, 4];
        #endregion

        #region Constructor
        public BuildActionButton(ActionFrame frame, UserInputManager manager, UnitType type,
            UnitTypeRegistry registry, UnitsRenderer unitsRenderer)
            : base(frame, manager, "Build", Keys.B)
        {
            Skills.Build buildingSkill = type.GetSkill<Skills.Build>();
            int x = 0;
            int y = 3;
            foreach (UnitType unitType in registry.Where(u => buildingSkill.Supports(u)))
            {
                Texture texture = unitsRenderer.GetTypeTexture(unitType);
                buildingButtons[x, y] = new BuildingConstructionActionButton(frame, manager, unitType, manager.Commander.Faction, texture);
                x++;
                if (x == 4)
                {
                    y--;
                    x = 0;
                }
            }

            buildingButtons[3, 0] = new CancelButton(frame, manager);
        }
        #endregion

        #region Methods
        public override ActionButton GetButtonAt(int x, int y)
        {
            return buildingButtons[x, y];
        }
        #endregion
    }
}
