using System;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A task which causes a unit (typically a building) to create a unit.
    /// </summary>
    [Serializable]
    public sealed class TrainTask : Task
    {
        #region Fields
        private readonly UnitType traineeType;
        private float healthPointsTrained;
        private bool attemptingToPlaceUnit;
        private bool waitingForEnoughFood;
        private bool hasEnded;
        #endregion

        #region Constructors
        public TrainTask(Unit trainer, UnitType traineeType)
            : base(trainer)
        {
            Argument.EnsureNotNull(trainer, "trainer");
            Argument.EnsureNotNull(traineeType, "traineeType");
            Argument.EnsureEqual(traineeType.IsBuilding, false, "traineeType.IsBuilding");
            if (trainer.IsUnderConstruction)
                throw new ArgumentException("Cannot train with an building under construction");

            if (!trainer.Type.HasSkill<TrainSkill>())
                throw new ArgumentException("Cannot train without the train skill.", "trainer");

            // Normally we'd check if the train skill supports the trainee type, but as the trainee type
            // can be a hero, which is not explicitely specified in the skill targets, that check has
            // been delegated to the TrainCommand level. Please update your bookmarks.

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
            get { return hasEnded; }
        }

        public UnitType TraineeType
        {
            get { return traineeType; }
        }

        public float Progress
        {
            get
            {
                int maxHealth = Unit.Faction.GetStat(traineeType, BasicSkill.MaxHealthStat);
                return Math.Min(healthPointsTrained / maxHealth, 1);
            }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (Unit.Faction.RemainingFoodAmount < Unit.Faction.GetStat(traineeType, BasicSkill.FoodCostStat))
            {
                if (!waitingForEnoughFood)
                {
                    waitingForEnoughFood = true;
                    string warning = "Pas assez de nourriture pour créer l'unité {0}".FormatInvariant(traineeType.Name);
                    Faction.RaiseWarning(warning);
                }

                return;
            }
            waitingForEnoughFood = false;

            float maxHealth = Unit.Faction.GetStat(traineeType, BasicSkill.MaxHealthStat);
            if (healthPointsTrained < maxHealth)
            {
                float trainingSpeed = Unit.GetStat(TrainSkill.SpeedStat);
                healthPointsTrained += trainingSpeed * step.TimeDeltaInSeconds;
                return;
            }

            Unit trainee = Unit.TrySpawn(traineeType);
            if (trainee != null)
            {
                attemptingToPlaceUnit = false;
                hasEnded = true;
            }
            else if (!attemptingToPlaceUnit)
            {
                attemptingToPlaceUnit = true;
                string warning = "Pas de place pour faire apparaître l'unité {0}".FormatInvariant(traineeType.Name);
                Faction.RaiseWarning(warning);
            }
        }
        #endregion
    }
}
