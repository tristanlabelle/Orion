using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class HealEnabler : ActionEnabler
    {
        public HealEnabler(UserInputManager manager, ActionFrame frame, TextureManager textureManager)
            : base(manager, frame, textureManager)
        {}

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<HealSkill>())
            {
                buttonsArray[3, 2] = new GenericActionButton(actionFrame, inputManager,
                    "Heal", Keys.H, new HealUserCommand(inputManager), base.textureManager);
            }
        }
    }
}