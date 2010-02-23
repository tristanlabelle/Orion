using System.Linq;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.Matchmaking;
using Orion.UserInterface.Widgets;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class TrainEnabler : ActionEnabler
    {
        #region Constructors
        public TrainEnabler(UserInputManager inputManager, ActionFrame frame, TextureManager textureManager)
            : base(inputManager, frame, textureManager)
        { }
        #endregion

        #region Methods
        public override void LetFill(UnitType unitType, ActionButton[,] buttonsArray)
        {
            TrainSkill trainSkill = unitType.GetSkill<TrainSkill>();
            if (trainSkill == null) return;
            
            int x = 0;
            int y = 3;
            foreach (UnitType traineeType in World.UnitTypes.Where(t => !t.IsBuilding && trainSkill.Supports(t)))
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

                ActionButton button = new ActionButton(actionFrame, inputManager, traineeType.Name, Keys.NoName, textureManager);

                Texture texture = textureManager.GetUnit(traineeType.Name);
                button.Renderer = new TexturedFrameRenderer(texture);

                UnitType traineeTypeForClosure = traineeType;
                button.Triggered += delegate(Button sender) { inputManager.LaunchTrain(traineeTypeForClosure); };

                int aladdium = LocalFaction.GetStat(traineeType, UnitStat.AladdiumCost);
                int alagene = LocalFaction.GetStat(traineeType, UnitStat.AlageneCost);
                button.Name = "{0}\nAladdium: {1} Alagene: {2}".FormatInvariant(traineeType.Name, aladdium, alagene);

                buttonsArray[x, y] = button;
            }
        }
        #endregion
    }
}
