using System;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.Matchmaking;
using Orion.UserInterface.Widgets;
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
        public HealEnabler(UICommander uiCommander, ActionFrame actionFrame, GameGraphics gameGraphics)
            : base(uiCommander, actionFrame, gameGraphics)
        {
            this.userCommand = new HealUserCommand(uiCommander);
        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<HealSkill>()) return;

            ActionButton button = new ActionButton(actionFrame, uiCommander, "Heal", Keys.H, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Heal");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += OnButtonPressed;

            buttonsArray[3, 2] = button;
        }

        private void OnButtonPressed(Button button)
        {
            uiCommander.SelectedCommand = userCommand;
            actionFrame.Push(new CancelActionProvider(actionFrame, uiCommander, gameGraphics));
        }
        #endregion
    }
}