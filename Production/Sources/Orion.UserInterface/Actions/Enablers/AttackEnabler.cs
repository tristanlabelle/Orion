using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;

namespace Orion.UserInterface.Actions.Enablers
{
    public class AttackEnabler : ActionEnabler
    {
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
