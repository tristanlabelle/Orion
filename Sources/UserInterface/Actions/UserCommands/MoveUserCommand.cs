using OpenTK.Math;
using Orion.Matchmaking;
using Orion.GameLogic;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class MoveUserCommand : UserInputCommand
    {
        public MoveUserCommand(UICommander uiCommander)
            : base(uiCommander) {}

        public override void OnClick(Vector2 location)
        {
            InputManager.LaunchMove(location);
        }
    }
}
