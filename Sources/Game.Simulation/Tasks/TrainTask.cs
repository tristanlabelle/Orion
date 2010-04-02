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

            TrainSkill trainSkill = trainer.Type.TryGetSkill<TrainSkill>();
            if (trainSkill == null)
                throw new ArgumentException("Cannot train without the train skill.", "trainer");
            if (!trainSkill.Supports(traineeType))
                throw new ArgumentException("Trainer {0} cannot train {1}.".FormatInvariant(trainer, traineeType));

            this.traineeType = TryGetHero(trainer.Faction.World.Random, traineeType, trainer.Faction.World.UnitTypes);
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
        private UnitType TryGetHero(Random random, UnitType unitType, UnitTypeRegistry registry)
        {
            while (unitType.HeroName != null && random.Next(0, 100) == 0)
            {
                UnitType heroUnitType = registry.FromName(unitType.HeroName);
                if (heroUnitType == null)
                {
#if DEBUG
                    Debug.Fail("Failed to retreive hero unit type {0} for unit type {1}."
                        .FormatInvariant(unitType.HeroName, unitType.Name));
#endif
                    return unitType;
                }

                unitType = heroUnitType;
            }

            return unitType;
        }

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
