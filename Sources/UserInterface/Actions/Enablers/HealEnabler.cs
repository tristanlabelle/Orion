using System;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Graphics.Renderers;
using Orion.Matchmaking;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
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