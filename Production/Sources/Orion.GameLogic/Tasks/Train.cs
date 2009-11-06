﻿using System;
using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    [Serializable]
    public sealed class Train : Task
    {
        #region Fields
        private readonly Unit trainer;
        private readonly UnitType traineeType;
        private float healthPointsTrained = 0;
        private bool hasTrainingBegun = false;
        private bool hasEnded = false;
        #endregion

        #region Constructors
        public Train(Unit trainer, UnitType traineeType)
        {
            Argument.EnsureNotNull(trainer, "trainer");
            if (!trainer.HasSkill<Skills.Train>())
                throw new ArgumentException("Cannot train without the train skill.", "trainer");
            Argument.EnsureNotNull(traineeType, "traineeType");
            Argument.EnsureEqual(traineeType.IsBuilding, false, "traineeType.IsBuilding");

            this.trainer = trainer;
            this.traineeType = traineeType;
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Training"; }
        }

        public override bool HasEnded
        {
            get
            {
                return hasEnded;
            }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (HasEnded) return;

            if (!hasTrainingBegun)
            {
                int aladdiumCost = trainer.Faction.GetStat(traineeType, UnitStat.AladdiumCost);
                int alageneCost = trainer.Faction.GetStat(traineeType, UnitStat.AlageneCost);

                if (trainer.Faction.AladdiumAmount >= aladdiumCost
                    && trainer.Faction.AlageneAmount >= alageneCost)
                {
                    trainer.Faction.AladdiumAmount -= aladdiumCost;
                    trainer.Faction.AlageneAmount -= alageneCost;
                    hasTrainingBegun = true;
                }
                else
                {
                    Console.WriteLine("Not Enough Resources");
                    hasEnded = true;
                    return;
                }
            }

            if (hasTrainingBegun)
            {
                float maxHealth = trainer.Faction.GetStat(traineeType, UnitStat.MaxHealth);
                float trainingSpeed = trainer.GetStat(UnitStat.TrainingSpeed);
                healthPointsTrained += trainingSpeed * timeDelta;
                if (healthPointsTrained >= maxHealth)
                {
                    // TODO: Refactor to take building size into account and position unit intelligently
                    Vector2 newPosition = new Vector2(trainer.Position.X + 2, trainer.Position.Y + 2);

                    // If the new assigned position is unavalible put it over the building
                    if (!trainer.World.IsWithinBounds(newPosition)
                        || !trainer.World.Terrain.IsWalkable(newPosition))
                        newPosition = trainer.Position;
                    
                    Unit unitCreated = trainer.Faction.CreateUnit(traineeType, newPosition);
                    unitCreated.Task = new Move(unitCreated, trainer.Position + trainer.RallyPoint); 
                  
                    hasEnded = true;
                }
            }
        }
        #endregion
    }
}
