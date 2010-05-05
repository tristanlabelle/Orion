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
    public sealed class DisembarkEnabler : ActionEnabler
    {
        #region Constructors
        public DisembarkEnabler(UserInputManager manager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(manager, actionPanel, gameGraphics)
        { }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<TransportSkill>()) return;

            ActionButton button = new ActionButton(actionPanel, userInputManager, "Débarquer", Keys.D, graphics);

            Texture texture = graphics.GetActionTexture("Disembark");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                userInputManager.LaunchDisembark();
            };

            buttonsArray[3, 0] = button;
        }
        #endregion
    }
}
