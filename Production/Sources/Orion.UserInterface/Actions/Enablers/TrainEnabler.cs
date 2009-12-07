using System.Linq;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;
using Orion.Graphics;

namespace Orion.UserInterface.Actions.Enablers
{
    public class TrainEnabler : ActionEnabler
    {
        private UnitTypeRegistry registry;

        public TrainEnabler(UserInputManager inputManager, ActionFrame frame, UnitTypeRegistry typeRegistry, TextureManager textureManager)
            : base(inputManager, frame, textureManager)
        {
            registry = typeRegistry;
        }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<Skills.TrainSkill>())
            {
                Skills.TrainSkill train = type.GetSkill<Skills.TrainSkill>();
                int x = 0;
                int y = 3;
                Faction playerFaction = inputManager.Commander.Faction;
                foreach (UnitType unitType in registry.Where(t => !t.IsBuilding))
                {
                    if (train.Supports(unitType))
                    {
                        // find an empty slot
                        while (buttonsArray[x, y] != null)
                        {
                            x++;
                            if (x == 4)
                            {
                                x = 0;
                                y--;
                            }
                        }

                        ImmediateUserCommand command = new TrainUserCommand(inputManager, unitType);
                        TrainActionButton button = new TrainActionButton(container, inputManager,unitType.Name, Keys.None, command, textureManager);
                        int aladdium = playerFaction.GetStat(unitType, UnitStat.AladdiumCost);
                        int alagene = playerFaction.GetStat(unitType, UnitStat.AlageneCost);
                        button.Name = "{0}\nAladdium: {1} Alagene: {2}".FormatInvariant(unitType.Name, aladdium, alagene);
                        buttonsArray[x, y] = button;
                    }
                }
            }
        }
    }
}
