using System.Linq;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Orion.Graphics;
using Orion.GameLogic.Skills;

namespace Orion.UserInterface.Actions.Enablers
{
    public class TrainEnabler : ActionEnabler
    {
        public TrainEnabler(UserInputManager inputManager, ActionFrame frame, TextureManager textureManager)
            : base(inputManager, frame, textureManager)
        { }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            TrainSkill trainSkill = type.GetSkill<TrainSkill>();
            if (trainSkill == null) return;
            
            int x = 0;
            int y = 3;
            foreach (UnitType unitType in World.UnitTypes.Where(t => !t.IsBuilding && trainSkill.Supports(t)))
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
                TrainActionButton button = new TrainActionButton(actionFrame, inputManager,unitType.Name, Keys.None, command, textureManager);
                int aladdium = LocalFaction.GetStat(unitType, UnitStat.AladdiumCost);
                int alagene = LocalFaction.GetStat(unitType, UnitStat.AlageneCost);
                button.Name = "{0}\nAladdium: {1} Alagene: {2}".FormatInvariant(unitType.Name, aladdium, alagene);
                buttonsArray[x, y] = button;
            }
        }
    }
}
