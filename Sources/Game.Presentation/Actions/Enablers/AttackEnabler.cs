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
    public sealed class AttackEnabler : ActionEnabler
    {
        #region Fields
        private readonly AttackUserCommand userCommand;
        #endregion

        #region Constructors
        public AttackEnabler(UserInputManager manager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(manager, actionPanel, gameGraphics)
        {
            this.userCommand = new AttackUserCommand(inputManager, gameGraphics);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<AttackSkill>()) return;

            ActionButton button = new ActionButton(actionPanel, inputManager, "Attack", Keys.A, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Attack");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[2, 3] = button;
        }

        private void OnButtonPressed(Button button)
        {
            inputManager.SelectedCommand = userCommand;
            actionPanel.Push(new CancelActionProvider(actionPanel, inputManager, gameGraphics));
        }
        #endregion
    }
}
