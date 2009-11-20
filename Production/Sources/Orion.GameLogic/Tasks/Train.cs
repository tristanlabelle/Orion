using System;
using OpenTK.Math;
using Orion.GameLogic;

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

            float maxHealth = trainer.Faction.GetStat(traineeType, UnitStat.MaxHealth);
            float trainingSpeed = trainer.GetStat(UnitStat.TrainingSpeed);
            healthPointsTrained += trainingSpeed * timeDelta;
            if (healthPointsTrained >= maxHealth)
            {
                if (trainer.Faction.AvailableFood < traineeType.FoodCost) return;
                Unit unitCreated = trainer.Faction.CreateUnit(traineeType, trainer.Position);
                unitCreated.Task = new Move(unitCreated, trainer.RallyPoint);
                hasEnded = true;
            }
        }
        #endregion
    }
}
