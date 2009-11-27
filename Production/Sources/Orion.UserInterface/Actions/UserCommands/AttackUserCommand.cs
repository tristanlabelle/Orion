using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Geometry;
using Orion.Commandment;
using OpenTK.Math;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class AttackUserCommand : UserInputCommand
    {
        private UserInputManager inputManager;

        public AttackUserCommand(UserInputManager manager)
        {
            inputManager = manager;
        }

        public override void Execute(Entity entity)
        {
            if (entity is Unit) inputManager.LaunchAttack((Unit)entity);
            else Execute(entity.Position);
        }

        public override void Execute(Vector2 point)
        {
            inputManager.LaunchZoneAttack(point);
        }
    }
}
