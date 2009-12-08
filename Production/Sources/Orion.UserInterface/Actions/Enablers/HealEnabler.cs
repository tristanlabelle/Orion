using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;
using Orion.Graphics;

namespace Orion.UserInterface.Actions.Enablers
{
    public class HealEnabler : ActionEnabler
    {
        private UnitTypeRegistry registry;

        public HealEnabler(UserInputManager manager, ActionFrame frame, UnitTypeRegistry entityRegistry, TextureManager textureManager)
            : base(manager, frame, textureManager)
        {
            registry = entityRegistry;
        }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<Skills.HealSkill>())
            {
                buttonsArray[2, 3] = new GenericActionButton(container, inputManager,
                    "Heal", Keys.H, new HealUserCommand(inputManager), base.textureManager);
            }
        }
    }
}