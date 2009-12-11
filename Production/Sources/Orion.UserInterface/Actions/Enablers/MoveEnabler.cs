using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public class MoveEnabler : ActionEnabler
    {
        public MoveEnabler(UserInputManager manager, ActionFrame frame, TextureManager textureManager)
            : base(manager, frame, textureManager)
        { }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<MoveSkill>())
            {
                buttonsArray[0, 3] = new GenericActionButton(container, inputManager,
                    "Move", Keys.M, new MoveUserCommand(inputManager), base.textureManager);
            }
        }
    }
}
