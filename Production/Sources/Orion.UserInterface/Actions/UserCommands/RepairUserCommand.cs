using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class RepairUserCommand : UserInputCommand
    {
        private UserInputManager inputManager;

        public RepairUserCommand(UserInputManager manager)
        {
            inputManager = manager;
        }

        public override void Execute(Entity entity)
        {
            if (entity is Unit) inputManager.LaunchRepair((Unit)entity);
            else Execute(entity.Position);
        }

        public override void Execute(Vector2 point)
        {
        }
    }
}
