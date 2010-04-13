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
        public BuildEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics graphics)
            : base(inputManager, actionPanel, graphics)
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
            ActionButton button = new ActionButton(actionPanel, userInputManager, "Build", Keys.B, graphics);

            Texture texture = graphics.GetActionTexture("Build");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                actionPanel.Push(new BuildActionProvider(actionPanel, userInputManager, graphics, unitType));
            };

            return button;
        }

        private ActionButton CreateRepairButton()
        {
            ActionButton button = new ActionButton(actionPanel, userInputManager, "Repair", Keys.R, graphics);

            Texture texture = graphics.GetActionTexture("Repair");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                userInputManager.SelectedCommand = repairUserCommand;
                actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
            };

            return button;
        }
        #endregion
    }
}
