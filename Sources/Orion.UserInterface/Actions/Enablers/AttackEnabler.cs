using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

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
            if (!type.HasSkill<AttackSkill>()) return;
            
            buttonsArray[2, 3] = new GenericActionButton(actionFrame, inputManager,
                "Attack", Keys.A, new AttackUserCommand(inputManager), base.textureManager);
        }
        #endregion
    }
}
