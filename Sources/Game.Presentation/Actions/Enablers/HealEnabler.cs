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
    public sealed class HealEnabler : ActionEnabler
    {
        #region Fields
        private readonly HealUserCommand userCommand;
        #endregion

        #region Constructors
        public HealEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(inputManager, actionPanel, gameGraphics)
        {
            this.userCommand = new HealUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<HealSkill>()) return;

            ActionButton button = new ActionButton(actionPanel, inputManager, "Heal", Keys.H, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Heal");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[3, 2] = button;
        }

        private void OnButtonPressed(Button button)
        {
            inputManager.SelectedCommand = userCommand;
            actionPanel.Push(new CancelActionProvider(actionPanel, inputManager, gameGraphics));
        }
        #endregion
    }
}