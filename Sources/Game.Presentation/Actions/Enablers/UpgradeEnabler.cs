using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Simulation;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class UpgradeEnabler : ActionEnabler
    {
        #region Constructors
        public UpgradeEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(inputManager, actionPanel, gameGraphics)
        {}
        #endregion

        #region Methods
        public override void LetFill(UnitType unitType, ActionButton[,] buttonsArray)
        {
            if (unitType.Upgrades.All(u => u.IsFree)) return;

            buttonsArray[2, 0] = CreateUpgradeButton(unitType);
        }

        private ActionButton CreateUpgradeButton(UnitType unitType)
        {
            ActionButton button = new ActionButton(actionPanel, userInputManager, "Upgrade", Keys.None, graphics);

            Texture texture = graphics.GetActionTexture("Upgrade");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                actionPanel.Push(new UpgradeActionProvider(actionPanel, userInputManager, graphics, unitType));
            };

            return button;
        }
        #endregion
    }
}
