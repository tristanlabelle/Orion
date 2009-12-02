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
        public AttackEnabler(UserInputManager manager, ActionFrame frame, TextureManager textureManager)
            : base(manager, frame, textureManager)
        { }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<Skills.Attack>())
            {
                buttonsArray[2, 3] = new GenericActionButton(container, inputManager,
                    "Attack", Keys.A, new AttackUserCommand(inputManager), base.textureManager);
            }
        }
        #endregion
    }
}
