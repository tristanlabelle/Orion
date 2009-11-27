using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;
using Orion.Graphics;

namespace Orion.UserInterface.Actions.Enablers
{
    public class MoveEnabler : ActionEnabler
    {
        public MoveEnabler(UserInputManager manager, ActionFrame frame)
            : base(manager, frame)
        { }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray, UnitsRenderer unitsRenderer)
        {
            if (type.HasSkill<Skills.Move>())
            {
                buttonsArray[0, 3] = new GenericActionButton(container, inputManager,
                    "Move", Keys.M, new MoveUserCommand(inputManager), null);
            }
        }
    }
}
