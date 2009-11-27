using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class MoveUserCommand : UserInputCommand
    {
        private UserInputManager inputManager;

        public MoveUserCommand(UserInputManager manager)
        {
            inputManager = manager;
        }

        public override void Execute(Entity entity)
        {
            Execute(entity.Position);
        }

        public override void Execute(Vector2 point)
        {
            inputManager.LaunchMove(point);
        }
    }
}
