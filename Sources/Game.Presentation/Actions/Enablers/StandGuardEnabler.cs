﻿using System;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Presentation.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class StandGuardEnabler : ActionEnabler
    {
        #region Constructors
        public StandGuardEnabler(UserInputManager manager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(manager, actionPanel, gameGraphics)
        {}
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<AttackSkill>() || !type.HasSkill<MoveSkill>()) return;

            ActionButton button = new ActionButton(actionPanel, inputManager, "Stand Guard", Keys.G, gameGraphics);

            Texture texture = gameGraphics.GetActionTexture("Stand Guard");
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                inputManager.LaunchStandGuard();
            };

            buttonsArray[3, 3] = button;
        }
        #endregion
    }
}
