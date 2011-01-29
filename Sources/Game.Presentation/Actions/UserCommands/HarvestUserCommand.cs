using System.Linq;
using OpenTK;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public sealed class HarvestUserCommand : UserInputCommand
    {
        public HarvestUserCommand(UserInputManager inputManager)
            : base(inputManager) {}

        public override void OnClick(Vector2 location)
        {
            Point point = (Point)location;
            if (!World.IsWithinBounds(point)) return;

            if (LocalFaction.GetTileVisibility(point) == TileVisibility.Visible)
            {
                Entity resourceNode = World.Entities
                    .Intersecting(location)
                    .Where(e => e.HasComponent<Harvestable>())
                    .FirstOrDefault(node => LocalFaction.CanHarvest(node));
                if (resourceNode == null) return;

                InputManager.LaunchHarvest(resourceNode);
            }
            else
            {
                InputManager.LaunchMove(location);
            }
        }
    }
}
