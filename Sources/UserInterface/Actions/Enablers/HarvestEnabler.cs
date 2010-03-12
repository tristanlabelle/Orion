using System;
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
    public sealed class HarvestEnabler : ActionEnabler
    {
        #region Fields
        private readonly HarvestUserCommand userCommand;
        #endregion

        #region Constructors
        public HarvestEnabler(UICommander uiCommander, ActionFrame actionFrame, GameGraphics gameGraphics)
            : base(uiCommander, actionFrame, gameGraphics)
        {
            this.userCommand = new HarvestUserCommand(uiCommander);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<HarvestSkill>()) return;

            ActionButton button = new ActionButton(actionFrame, uiCommander, "Harvest", Keys.H, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Harvest");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[1, 2] = button;
        }

        private void OnButtonPressed(Button button)
        {
            uiCommander.SelectedCommand = userCommand;
            actionFrame.Push(new CancelActionProvider(actionFrame, uiCommander, gameGraphics));
        }
        #endregion
    }
}
