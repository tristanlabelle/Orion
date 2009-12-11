﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions
{
    public class BuildActionButton : ActionButton
    {
        #region Fields
        private ActionButton[,] buildingButtons = new ActionButton[4, 4];
        #endregion

        #region Constructor
        public BuildActionButton(ActionFrame frame, UserInputManager manager, UnitType type,
            UnitTypeRegistry registry, TextureManager textureManager)
            : base(frame, manager, "Build", Keys.B, textureManager)
        {
            BuildSkill buildSkill = type.GetSkill<BuildSkill>();
            int x = 0;
            int y = 3;
            foreach (UnitType unitType in registry.Where(u => buildSkill.Supports(u)))
            {
                buildingButtons[x, y] = new BuildingConstructionActionButton(frame, manager, unitType, manager.LocalCommander.Faction, textureManager);
                x++;
                if (x == 4)
                {
                    y--;
                    x = 0;
                }
            }

            buildingButtons[3, 0] = new CancelButton(frame, manager, textureManager);

            Texture texture = textureManager.GetAction("Build");
            base.Renderer = new TexturedFrameRenderer(texture);
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
