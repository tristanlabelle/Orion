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

namespace Orion.UserInterface.Actions.Enablers
{
    public class BuildEnabler : ActionEnabler
    {
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
            }
        }
    }
}
