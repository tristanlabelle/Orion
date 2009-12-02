using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;
using Orion.Graphics;

namespace Orion.UserInterface.Actions.Enablers
{
    public class HarvestEnabler : ActionEnabler
    {
        public HarvestEnabler(UserInputManager manager, ActionFrame frame, TextureManager textureManager)
            : base(manager, frame, textureManager)
        { }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<Skills.Harvest>())
            {
                buttonsArray[1, 2] = new GenericActionButton(container, inputManager,
                    "Harvest", Keys.H, new HarvestUserCommand(inputManager), base.textureManager);
            }
        }
    }
}
