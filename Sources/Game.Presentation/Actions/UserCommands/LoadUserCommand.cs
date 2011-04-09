using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public class LoadUserCommand : UserInputCommand
    {
        public LoadUserCommand(UserInputManager inputManager)
            : base(inputManager) {}

        public override void OnClick(Vector2 location)
        {
            Point point = (Point)location;
            if (!World.IsWithinBounds(point)) return;

            if (LocalFaction.GetTileVisibility(point) == TileVisibility.Visible)
            {
                Entity target = World.Entities.GetTopmostGridEntityAt(point);
                if (target == null
                    || !target.Components.Has<FactionMembership>()
                    || target.Components.Get<FactionMembership>().Faction != InputManager.LocalFaction)
                {
                    // can't load this kind of thing
                    return;
                }

                Entity transporter = InputManager.Selection
                    .Where(e => Identity.GetPrototype(e) == InputManager.SelectionManager.FocusedPrototype)
                    .Where(e => e.Components.Has<Transporter>())
                    .Where(e => e.Components.Get<Transporter>().RemainingSpace >= target.Components.Get<Cost>().Food)
                    .FirstOrDefault();
                if (transporter == null) return;

                InputManager.LaunchLoad(target);
            }
        }
    }
}
