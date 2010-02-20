using OpenTK.Math;
using Orion.Matchmaking;
using Orion.GameLogic;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class HealUserCommand : UserInputCommand
    {
        public HealUserCommand(UserInputManager inputManager)
            : base(inputManager) {}

        public override void OnClick(Vector2 location)
        {
            Point point = (Point)location;;
            if (!World.IsWithinBounds(point)) return;

            Unit target = World.Entities.GetUnitAt(point);
            if (target != null) InputManager.LaunchHeal(target);
        }
    }
}
