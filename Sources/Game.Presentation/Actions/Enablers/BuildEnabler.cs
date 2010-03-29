using System;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class BuildEnabler : ActionEnabler
    {
        #region Fields
        private readonly RepairUserCommand repairUserCommand;
        #endregion

        #region Constructors
        public BuildEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(inputManager, actionPanel, gameGraphics)
        {
            this.repairUserCommand = new RepairUserCommand(inputManager);
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
            ActionButton button = new ActionButton(actionPanel, inputManager, "Build", Keys.B, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Build");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                actionPanel.Push(new BuildActionProvider(actionPanel, inputManager, unitType, World.UnitTypes, gameGraphics));
            };

            return button;
        }

        private ActionButton CreateRepairButton()
        {
            ActionButton button = new ActionButton(actionPanel, inputManager, "Repair", Keys.R, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Repair");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                inputManager.SelectedCommand = repairUserCommand;
                actionPanel.Push(new CancelActionProvider(actionPanel, inputManager, gameGraphics));
            };

            return button;
        }
        #endregion
    }
}
