using System;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class AttackUserCommand : UserInputCommand
    {
        public AttackUserCommand(UserInputManager inputManager)
            : base(inputManager)
        { }

        public override void OnClick(Vector2 location)
        {
            Unit target = World.Entities.GetUnitAt((Point)location);
            if (target == null) InputManager.LaunchZoneAttack(location);
            else InputManager.LaunchAttack(target);
        }
    }
}
