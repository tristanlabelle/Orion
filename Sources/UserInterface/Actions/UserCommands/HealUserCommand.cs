using OpenTK.Math;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public sealed class HealUserCommand : UserInputCommand
    {
        public HealUserCommand(UserInputManager inputManager)
            : base(inputManager) {}

        public override void OnClick(Vector2 location)
        {
            Point point = (Point)location;;
            if (!World.IsWithinBounds(point)) return;

            Unit target = World.Entities.GetTopmostUnitAt(point);
            if (target != null) InputManager.LaunchHeal(target);
        }
    }
}
