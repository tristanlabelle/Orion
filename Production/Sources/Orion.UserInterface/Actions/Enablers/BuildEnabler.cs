using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;

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
                buttonsArray[1, 0] = new GenericActionButton(container, inputManager, "Repair", Keys.R, new RepairUserCommand(inputManager));
            }
        }
    }
}
