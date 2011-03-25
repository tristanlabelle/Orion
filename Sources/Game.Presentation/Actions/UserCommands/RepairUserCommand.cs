using OpenTK;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;

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

            if (LocalFaction.GetTileVisibility(point) == TileVisibility.Visible)
            {
                Entity target = World.Entities.GetTopmostGridEntityAt(point);
                if (target != null) InputManager.LaunchRepair(target);
            }
            else
            {
                InputManager.LaunchMove(location);
            }
        }
    }
}
