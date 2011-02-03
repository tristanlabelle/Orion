using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Skills;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Keys = System.Windows.Forms.Keys;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class SellEnabler : ActionEnabler
    {
        #region Constructors
        public SellEnabler(UserInputManager manager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(manager, actionPanel, gameGraphics)
        { }
        #endregion

        #region Methods
        public override void LetFill(Unit type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<SellableSkill>()) return;

            ActionButton button = new ActionButton(actionPanel, userInputManager, "Sell", Keys.None, graphics);

            Texture texture = graphics.GetActionTexture("Sell");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                userInputManager.LaunchSell();
            };

            buttonsArray[3, 0] = button;
        }
        #endregion
    }
}
