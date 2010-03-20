using System;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Graphics.Renderers;
using Orion.Matchmaking;
using Orion.UserInterface.Actions.UserCommands;
using Orion.UserInterface.Widgets;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class HarvestEnabler : ActionEnabler
    {
        #region Fields
        private readonly HarvestUserCommand userCommand;
        #endregion

        #region Constructors
        public HarvestEnabler(UserInputManager inputManager, ActionFrame actionFrame, GameGraphics gameGraphics)
            : base(inputManager, actionFrame, gameGraphics)
        {
            this.userCommand = new HarvestUserCommand(inputManager);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill(UnitSkill.Harvest)) return;

            ActionButton button = new ActionButton(actionFrame, inputManager, "Harvest", Keys.H, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Harvest");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[1, 2] = button;
        }

        private void OnButtonPressed(Button button)
        {
            inputManager.SelectedCommand = userCommand;
            actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, gameGraphics));
        }
        #endregion
    }
}
