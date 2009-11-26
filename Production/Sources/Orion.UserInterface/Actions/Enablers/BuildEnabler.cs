using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Skills = Orion.GameLogic.Skills;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Geometry;
using Orion.Commandment;

namespace Orion.UserInterface.Actions.Enablers
{
    public class BuildEnabler : ActionEnabler
    {
        #region Nested Types
        private class RepairUserCommand : UserInputCommand
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
                // todo: don't fail silently
            }
        }
        #endregion

        private UnitTypeRegistry registry;

        public BuildEnabler(UserInputManager manager, ActionFrame frame, UnitTypeRegistry entityRegistry)
            : base(manager, frame)
        {
            registry = entityRegistry;
        }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<Skills.Build>())
            {
                buttonsArray[0, 0] = new BuildActionButton(container, inputManager, type, registry);
                buttonsArray[1, 0] = new GenericActionButton(container, inputManager, "Repair", Keys.R, new RepairUserCommand(inputManager));
            }
        }
    }
}
