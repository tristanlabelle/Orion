using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;

namespace Orion.UserInterface.Actions.UserCommands
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
