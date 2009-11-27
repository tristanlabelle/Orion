using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;
using Orion.Graphics;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class AttackEnabler : ActionEnabler
    {
        #region Constructors
        public AttackEnabler(UserInputManager manager, ActionFrame frame)
            : base(manager, frame)
        { }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray, UnitsRenderer unitsRenderer)
        {
            if (type.HasSkill<Skills.Attack>())
            {
                buttonsArray[2, 3] = new GenericActionButton(container, inputManager,
                    "Attack", Keys.A, new AttackUserCommand(inputManager), null);
            }
        }
        #endregion
    }
}
