using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;

namespace Orion.UserInterface.Actions.Enablers
{
    public class HarvestEnabler : ActionEnabler
    {
        public HarvestEnabler(UserInputManager manager, ActionFrame frame)
            : base(manager, frame)
        { }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<Skills.Harvest>())
                buttonsArray[1, 2] = new GenericActionButton(container, inputManager, "Harvest", Keys.H, new HarvestUserCommand(inputManager));
        }
    }
}
