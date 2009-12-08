using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class HealUserCommand : UserInputCommand
    {
        private UserInputManager inputManager;

        public HealUserCommand(UserInputManager manager)
        {
            inputManager = manager;
        }

        public override void Execute(Entity entity)
        {
            if (entity is Unit) inputManager.LaunchHeal((Unit)entity);
            else Execute(entity.Position);
        }

        public override void Execute(Vector2 point)
        {
            // todo: don't fail silently
        }
    }
}
