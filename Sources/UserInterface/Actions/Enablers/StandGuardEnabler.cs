using System;
using Orion.Engine.Graphics;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Graphics.Renderers;
using Orion.UserInterface.Actions.UserCommands;
using Orion.UserInterface.Widgets;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class StandGuardEnabler : ActionEnabler
    {
        #region Constructors
        public StandGuardEnabler(UserInputManager manager, ActionFrame frame, GameGraphics gameGraphics)
            : base(manager, frame, gameGraphics)
        {}
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill(UnitSkill.Attack) || !type.HasSkill(UnitSkill.Move)) return;

            ActionButton button = new ActionButton(actionFrame, inputManager, "Stand Guard", Keys.G, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Stand Guard");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                inputManager.LaunchStandGuard();
            };

            buttonsArray[3, 3] = button;
        }
        #endregion
    }
}
