﻿using System.Linq;
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
        public override void LetFill(UnitType unitType, ActionButton[,] buttonsArray)
        {
            TrainSkill trainSkill = unitType.TryGetSkill<TrainSkill>();
            if (trainSkill == null) return;
            
            int x = 0;
            int y = 3;


            var traineeTypes = Match.UnitTypes
                .Where(traineeType => trainSkill.Supports(traineeType))
                .OrderBy(traineeType => traineeType.GetBaseStat(BasicSkill.AladdiumCostStat) + traineeType.GetBaseStat(BasicSkill.AlageneCostStat));

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

                int aladdium = LocalFaction.GetStat(traineeType, BasicSkill.AladdiumCostStat);
                int alagene = LocalFaction.GetStat(traineeType, BasicSkill.AlageneCostStat);
                
                UnitType traineeTypeForClosure = traineeType;
                ActionButton button = new ActionButton()
                {
                	Name = traineeType.Name,
                	Description = "Aladdium: {1} Alagene: {2}".FormatInvariant(aladdium, alagene),
                	Texture = graphics.GetUnitTexture(traineeType),
                	Action = () => userInputManager.LaunchTrain(traineeTypeForClosure)
                };

                buttonsArray[x, y] = button;
            }
        }
        #endregion
    }
}
