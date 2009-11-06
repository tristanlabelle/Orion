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

namespace Orion.UserInterface.Actions.Enablers
{
    public class AttackEnabler : ActionEnabler
    {
        #region Nested Types
        private class AttackUserCommand : UserInputCommand
        {
            private UserInputManager inputManager;

            public AttackUserCommand(UserInputManager manager)
            {
                inputManager = manager;
            }

            public override void Execute(Entity entity)
            {
                if (entity is Unit) inputManager.LaunchAttack((Unit)entity);
                else Execute(entity.BoundingRectangle.Center);
            }

            public override void Execute(Vector2 point)
            {
                inputManager.LaunchZoneAttack(point);
            }
        }
        #endregion

        public AttackEnabler(UserInputManager manager, ActionFrame frame)
            : base(manager, frame)
        { }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<Skills.Attack>())
                buttonsArray[2, 3] = new GenericActionButton(container, inputManager, "Attack", Keys.A, new AttackUserCommand(inputManager));
        }
    }
}
