using OpenTK;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public sealed class MoveUserCommand : UserInputCommand
    {
        public MoveUserCommand(UserInputManager inputManager)
            : base(inputManager) {}

        public override void OnClick(Vector2 location)
        {
            InputManager.LaunchMove(location);
        }
    }
}
