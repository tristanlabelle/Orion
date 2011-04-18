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
                Spatial resourceNodeSpatial = World.SpatialManager
                    .Intersecting(location)
                    .FirstOrDefault(spatial => LocalFaction.CanHarvest(spatial.Entity));
                if (resourceNodeSpatial == null) return;

                InputManager.LaunchHarvest(resourceNodeSpatial.Entity);
            }
            else
            {
                InputManager.LaunchMove(location);
            }
        }
    }
}
