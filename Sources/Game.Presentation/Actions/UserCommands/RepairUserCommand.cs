using System;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public sealed class RepairUserCommand : UserInputCommand
    {
        public RepairUserCommand(UserInputManager inputManager)
            : base(inputManager) {}

        public override void OnClick(Vector2 location)
        {
            Point point = (Point)location;
            if (!World.IsWithinBounds(point)) return;

            Entity target = GetTopmostEntityWhere(location, entity =>
                {
                    Health health = entity.Components.TryGet<Health>();
                    return health != null
                        && health.Constitution == Constitution.Mechanical
                        && FactionMembership.GetFaction(entity) == LocalFaction;
                });

            if (target != null && LocalFaction.GetTileVisibility(point) == TileVisibility.Visible)
                InputManager.LaunchRepair(target);
            else
                InputManager.LaunchMove(location);
        }
    }
}
