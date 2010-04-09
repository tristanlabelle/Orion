using System.Linq;
using OpenTK.Math;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Engine;

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
                ResourceNode resourceNode = World.Entities
                    .Intersecting(location)
                    .OfType<ResourceNode>()
                    .FirstOrDefault(node => node.IsHarvestableByFaction(LocalFaction));
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
