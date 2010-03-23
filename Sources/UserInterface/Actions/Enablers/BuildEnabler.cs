using System;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Renderers;
using Orion.Engine.Gui;
using Orion.Matchmaking;
using Orion.Game.Presentation.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class BuildEnabler : ActionEnabler
    {
        #region Fields
        private readonly RepairUserCommand repairUserCommand;
        #endregion

        #region Constructors
        public BuildEnabler(UserInputManager inputManager, ActionFrame actionFrame, GameGraphics gameGraphics)
            : base(inputManager, actionFrame, gameGraphics)
        {
            this.repairUserCommand = new RepairUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType unitType, ActionButton[,] buttonsArray)
        {
            if (!unitType.HasSkill(UnitSkill.Build)) return;
            
            buttonsArray[0, 0] = CreateBuildButton(unitType);
            buttonsArray[1, 0] = CreateRepairButton();
        }

        private ActionButton CreateBuildButton(UnitType unitType)
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, "Build", Keys.B, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Build");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                actionFrame.Push(new BuildActionProvider(actionFrame, inputManager, unitType, World.UnitTypes, gameGraphics));
            };

            return button;
        }

        private ActionButton CreateRepairButton()
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, "Repair", Keys.R, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Repair");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                inputManager.SelectedCommand = repairUserCommand;
                actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, gameGraphics));
            };

            return button;
        }
        #endregion
    }
}
