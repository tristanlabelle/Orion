﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Engine;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public sealed class EmbarkUserCommand : UserInputCommand
    {
        public EmbarkUserCommand(UserInputManager inputManager)
            : base(inputManager) { }

        public override void OnClick(Vector2 location)
        {
            Unit target = LocalFaction.Units
                .FirstOrDefault(unit => unit.BoundingRectangle.ContainsPoint(location) && !unit.IsBuilding);
            if (target == null) return;

            InputManager.LaunchEmbark(target);
        }
    }
}
