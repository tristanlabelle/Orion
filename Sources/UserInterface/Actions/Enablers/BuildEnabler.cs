﻿using System;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.Matchmaking;
using Orion.UserInterface.Actions.UserCommands;
using Orion.UserInterface.Widgets;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class BuildEnabler : ActionEnabler
    {
        #region Fields
        private readonly RepairUserCommand repairUserCommand;
        #endregion

        #region Constructors
        public BuildEnabler(UICommander uiCommander, ActionFrame actionFrame, GameGraphics gameGraphics)
            : base(uiCommander, actionFrame, gameGraphics)
        {
            this.repairUserCommand = new RepairUserCommand(uiCommander);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType unitType, ActionButton[,] buttonsArray)
        {
            if (!unitType.HasSkill<BuildSkill>()) return;
            
            buttonsArray[0, 0] = CreateBuildButton(unitType);
            buttonsArray[1, 0] = CreateRepairButton();
        }

        private ActionButton CreateBuildButton(UnitType unitType)
        {
            ActionButton button = new ActionButton(actionFrame, uiCommander, "Build", Keys.B, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Build");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                actionFrame.Push(new BuildActionProvider(actionFrame, uiCommander, unitType, World.UnitTypes, gameGraphics));
            };

            return button;
        }

        private ActionButton CreateRepairButton()
        {
            ActionButton button = new ActionButton(actionFrame, uiCommander, "Repair", Keys.R, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Repair");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                uiCommander.SelectedCommand = repairUserCommand;
                actionFrame.Push(new CancelActionProvider(actionFrame, uiCommander, gameGraphics));
            };

            return button;
        }
        #endregion
    }
}
