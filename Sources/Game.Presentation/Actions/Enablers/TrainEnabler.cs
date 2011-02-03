using System.Linq;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class TrainEnabler : ActionEnabler
    {
        #region Constructors
        public TrainEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(inputManager, actionPanel, gameGraphics)
        { }
        #endregion

        #region Methods
        public override void LetFill(Unit unitType, ActionButton[,] buttonsArray)
        {
            TrainSkill trainSkill = unitType.TryGetSkill<TrainSkill>();
            if (trainSkill == null) return;
            
            int x = 0;
            int y = 3;


            var traineeTypes = Match.UnitTypes
                .Where(traineeType => trainSkill.Supports(traineeType))
                .OrderBy(traineeType => traineeType.GetBaseStat(BasicSkill.AladdiumCostStat) + traineeType.GetBaseStat(BasicSkill.AlageneCostStat));

            foreach (Unit traineeType in traineeTypes)
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

                ActionButton button = new ActionButton(actionPanel, userInputManager, traineeType.Name, Keys.None, graphics);

                Texture texture = graphics.GetUnitTexture(traineeType);
                button.Renderer = new TexturedRenderer(texture);

                Unit traineeTypeForClosure = traineeType;
                button.Triggered += delegate(Button sender) { userInputManager.LaunchTrain(traineeTypeForClosure); };

                int aladdium = LocalFaction.GetStat(traineeType, BasicSkill.AladdiumCostStat);
                int alagene = LocalFaction.GetStat(traineeType, BasicSkill.AlageneCostStat);
                button.Name = "{0}\nAladdium: {1} Alagene: {2}".FormatInvariant(traineeType.Name, aladdium, alagene);

                buttonsArray[x, y] = button;
            }
        }
        #endregion
    }
}
