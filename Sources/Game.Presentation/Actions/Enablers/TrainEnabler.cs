using System.Linq;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class TrainEnabler : ActionEnabler
    {
        #region Constructors
        public TrainEnabler(UserInputManager inputManager, ActionFrame frame, GameGraphics gameGraphics)
            : base(inputManager, frame, gameGraphics)
        { }
        #endregion

        #region Methods
        public override void LetFill(UnitType unitType, ActionButton[,] buttonsArray)
        {
            if (!unitType.HasSkill(UnitSkill.Train)) return;
            
            int x = 0;
            int y = 3;

            var traineeTypes = World.UnitTypes
                .Where(traineeType => unitType.CanTrain(traineeType))
                .OrderBy(traineeType => traineeType.GetBaseStat(UnitStat.AladdiumCost) + traineeType.GetBaseStat(UnitStat.AlageneCost));

            foreach (UnitType traineeType in traineeTypes)
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

                ActionButton button = new ActionButton(actionFrame, inputManager, traineeType.Name, Keys.None, gameGraphics);

                Texture texture = gameGraphics.GetUnitTexture(traineeType);
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
