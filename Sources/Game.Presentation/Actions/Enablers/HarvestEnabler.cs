using System;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class HarvestEnabler : ActionEnabler
    {
        #region Fields
        private readonly HarvestUserCommand userCommand;
        #endregion

        #region Constructors
        public HarvestEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(inputManager, actionPanel, gameGraphics)
        {
            this.userCommand = new HarvestUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(Unit type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<HarvestSkill>()) return;

            ActionButton button = new ActionButton(actionPanel, userInputManager, "Harvest", Keys.H, graphics);

            Texture texture = graphics.GetActionTexture("Harvest");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[1, 2] = button;
        }

        private void OnButtonPressed(Button button)
        {
            userInputManager.SelectedCommand = userCommand;
            actionPanel.Push(new CancelActionProvider(actionPanel, userInputManager, graphics));
        }
        #endregion
    }
}
