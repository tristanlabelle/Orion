using System;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Renderers;
using Orion.Matchmaking;
using Orion.Game.Presentation.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class HealEnabler : ActionEnabler
    {
        #region Fields
        private readonly HealUserCommand userCommand;
        #endregion

        #region Constructors
        public HealEnabler(UserInputManager inputManager, ActionFrame actionFrame, GameGraphics gameGraphics)
            : base(inputManager, actionFrame, gameGraphics)
        {
            this.userCommand = new HealUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill(UnitSkill.Heal)) return;

            ActionButton button = new ActionButton(actionFrame, inputManager, "Heal", Keys.H, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Heal");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[3, 2] = button;
        }

        private void OnButtonPressed(Button button)
        {
            inputManager.SelectedCommand = userCommand;
            actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, gameGraphics));
        }
        #endregion
    }
}