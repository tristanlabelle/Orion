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

            Entity target = GetTopmostEntityWhere(location, entity =>
                FactionMembership.GetFaction(entity) == LocalFaction
                && !entity.Identity.IsBuilding
                && !entity.Components.Has<Transporter>());

            if (target != null && LocalFaction.GetTileVisibility(point) == TileVisibility.Visible)
            {
                Entity transporter = InputManager.Selection
                    .FirstOrDefault(e =>
                        Identity.GetPrototype(e) == InputManager.SelectionManager.FocusedPrototype
                        && e.Components.Has<Transporter>()
                        && e.Components.Get<Transporter>().RemainingSpace >= target.Components.Get<Cost>().Food);
                if (transporter == null) return;

                InputManager.LaunchLoad(target);
            }
            else
            {
                InputManager.LaunchMove(location);
            }
        }
    }
}
