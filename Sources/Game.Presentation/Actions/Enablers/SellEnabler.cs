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
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<SellableSkill>()) return;
            
            buttonsArray[3, 0] = new ActionButton()
            {
            	Name = "Vendre",
            	Texture = graphics.GetActionTexture("Sell"),
            	Action = () => userInputManager.LaunchSell()
            };
        }
        #endregion
    }
}
