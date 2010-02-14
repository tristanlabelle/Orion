using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public class BuildEnabler : ActionEnabler
    {
        public BuildEnabler(UserInputManager manager, ActionFrame frame, TextureManager textureManager)
            : base(manager, frame, textureManager)
        { }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<BuildSkill>())
            {
                buttonsArray[0, 0] = new BuildActionButton(actionFrame, inputManager, type, World.UnitTypes, base.textureManager);
                buttonsArray[1, 0] = new GenericActionButton(actionFrame, inputManager,
                    "Repair", Keys.R, new RepairUserCommand(inputManager), base.textureManager);
            }
        }
    }
}
