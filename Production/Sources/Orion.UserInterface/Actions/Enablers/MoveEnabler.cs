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
    public class MoveEnabler : ActionEnabler
    {
        #region Nested Types
        private class MoveUserCommand : UserInputCommand
        {
            private UserInputManager inputManager;

            public MoveUserCommand(UserInputManager manager)
            {
                inputManager = manager;
            }

            public override void Target(Entity entity)
            {
                Target(entity.BoundingRectangle.Center);
            }

            public override void Target(Vector2 point)
            {
                inputManager.LaunchMove(point);
            }
        }
        #endregion

        public MoveEnabler(UserInputManager manager, ActionFrame frame)
            : base(manager, frame)
        { }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<Skills.Move>())
                buttonsArray[0, 3] = new GenericActionButton(container, inputManager, "Move", Keys.M, new MoveUserCommand(inputManager));
        }
    }
}
