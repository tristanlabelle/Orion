using System;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using System.Diagnostics;

namespace Orion.GameLogic.Tasks
{
    [Serializable]
    public sealed class Train : Task
    {
        #region Fields
        private readonly Unit trainer;
        private readonly UnitType traineeType;
        private float healthPointsTrained = 0;
        private bool hasEnded = false;
        #endregion

        #region Constructors
        public Train(Unit trainer, UnitType traineeType)
        {
            Argument.EnsureNotNull(trainer, "trainer");
            if (!trainer.HasSkill<Skills.Train>())
                throw new ArgumentException("Cannot train without the train skill.", "trainer");
            if (trainer.UnderConstruction)
                throw new ArgumentException("Cannot train with an Unit in Construction");
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

            if (trainer.Faction.RemainingFoodAmount < traineeType.FoodCost) return;

            float maxHealth = trainer.Faction.GetStat(traineeType, UnitStat.MaxHealth);
            float trainingSpeed = trainer.GetStat(UnitStat.TrainingSpeed);
            healthPointsTrained += trainingSpeed * timeDelta;
            if (healthPointsTrained >= maxHealth)
            {
                Point? spawnPoint = trainer.GridRegion.GetAdjacentPoints()
                    .FirstOrNull(point => trainer.World.IsWithinBounds(point) && trainer.World.IsTileFree(point));

                if (!spawnPoint.HasValue)
                {
                    Debug.Fail("No free point to spawn unit.");
                    spawnPoint = (Point)trainer.Position;
                }

                Unit unitCreated = trainer.Faction.CreateUnit(traineeType, spawnPoint.Value);
                unitCreated.Task = new Move(unitCreated, trainer.RallyPoint.Value);
                hasEnded = true;
            }
        }
        #endregion
    }
}
