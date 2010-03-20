using System;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;

namespace Orion.GameLogic.Tasks
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
            if (!trainer.HasSkill(UnitSkill.Train))
                throw new ArgumentException("Cannot train without the train skill.", "trainer");
            if (!trainer.Type.CanTrain(traineeType))
                throw new ArgumentException("Trainer {0} cannot train {1}.".FormatInvariant(trainer, traineeType));

            this.traineeType = TryTrainHero(trainer.Faction.World.Random, traineeType, trainer.Faction.World.UnitTypes);
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
                int maxHealth = Unit.Faction.GetStat(traineeType, UnitStat.MaxHealth);
                return Math.Min(healthPointsTrained / maxHealth, 1);
            }
        }
        #endregion

        #region Methods
        private UnitType TryTrainHero(Random random, UnitType traineeType, UnitTypeRegistry registry)
        {
            if (random.Next(0, 10000) == 1337)
            {
                return registry.FromName("Chuck Norris");
            }
            if (random.Next(0, 100) == 99)
            {
                if (traineeType.Name == "Schtroumpf")
                    return registry.FromName("Grand schtroumpf");
                if (traineeType.Name == "Pirate")
                    return registry.FromName("Barbe bleue");
                if (traineeType.Name == "Ninja")
                    return registry.FromName("Léonardo");
                if (traineeType.Name == "Viking")
                    return registry.FromName("Thor");
                if (traineeType.Name == "Jedihad")
                    return registry.FromName("Allah Skywalker");
                if (traineeType.Name == "Jésus")
                    return registry.FromName("Jésus-raptor");
                if (traineeType.Name == "Flying Spaghetti Monster")
                    return registry.FromName("Ta mère");
                if (traineeType.Name == "Grippe A(H1N1)")
                    return registry.FromName("Anthrax");
                if (traineeType.Name == "OVNI")
                    return registry.FromName("Vaisseau mère");
                if (traineeType.Name == "Tapis volant")
                    return registry.FromName("Tapis d'Aladdin");
            }
            return traineeType;
        }

        protected override void DoUpdate(SimulationStep step)
        {
            if (Unit.Faction.RemainingFoodAmount < Unit.Faction.GetStat(traineeType, UnitStat.FoodCost))
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

            float maxHealth = Unit.Faction.GetStat(traineeType, UnitStat.MaxHealth);
            if (healthPointsTrained < maxHealth)
            {
                float trainingSpeed = Unit.GetStat(UnitStat.TrainSpeed);
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
