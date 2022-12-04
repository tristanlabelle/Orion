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
        public TrainEnabler(UICommander uiCommander, ActionFrame frame, GameGraphics gameGraphics)
            : base(uiCommander, frame, gameGraphics)
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

                ActionButton button = new ActionButton(actionFrame, uiCommander, traineeType.Name, Keys.None, gameGraphics);

                Texture texture = gameGraphics.GetUnitTexture(traineeType);
                button.Renderer = new TexturedFrameRenderer(texture);

                UnitType traineeTypeForClosure = traineeType;
                button.Triggered += delegate(Button sender) { uiCommander.LaunchTrain(traineeTypeForClosure); };

                int aladdium = LocalFaction.GetStat(traineeType, UnitStat.AladdiumCost);
                int alagene = LocalFaction.GetStat(traineeType, UnitStat.AlageneCost);
                button.Name = "{0}\nAladdium: {1} Alagene: {2}".FormatInvariant(traineeType.Name, aladdium, alagene);

                buttonsArray[x, y] = button;
            }
        }
        #endregion
    }
}
