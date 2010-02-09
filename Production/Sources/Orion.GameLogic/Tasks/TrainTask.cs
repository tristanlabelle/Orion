using System;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.GameLogic.Skills;

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
            if (trainer.IsUnderConstruction)
                throw new ArgumentException("Cannot train with an Unit in Construction");
            TrainSkill trainSkill = trainer.GetSkill<TrainSkill>();
            if (trainSkill == null)
                throw new ArgumentException("Cannot train without the train skill.", "trainer");
            if (!trainSkill.Supports(traineeType))
                throw new ArgumentException("Trainer {0} cannot train {1}.".FormatInvariant(trainer, traineeType));
            Argument.EnsureNotNull(traineeType, "traineeType");
            Argument.EnsureEqual(traineeType.IsBuilding, false, "traineeType.IsBuilding");

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
                    return registry.FromName("Grand Schtroumpf");
                if (traineeType.Name == "Pirate")
                    return registry.FromName("Barbe Bleu");
                if (traineeType.Name == "Ninja")
                    return registry.FromName("Léonardo");
                if (traineeType.Name == "Viking")
                    return registry.FromName("Thor");
                if (traineeType.Name == "Jedihad")
                    return registry.FromName("Allah Skywalker");
                if (traineeType.Name == "Jésus")
                    return registry.FromName("Jésus-Raptor");
                if (traineeType.Name == "Flying Spaghetti Monster")
                    return registry.FromName("Ta Mère");
                if (traineeType.Name == "Grippe A(H1N1)")
                    return registry.FromName("Anthrax");
                if (traineeType.Name == "OVNI")
                    return registry.FromName("Vaisseau Mère");
                if (traineeType.Name == "Tapis Volant")
                    return registry.FromName("Le Tapis d'Aladdin");
            }
            return traineeType;
        }

        protected override void DoUpdate(SimulationStep step)
        {
            if (Unit.Faction.RemainingFoodAmount < traineeType.FoodCost)
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
                float trainingSpeed = Unit.GetStat(UnitStat.TrainingSpeed);
                healthPointsTrained += trainingSpeed * step.TimeDeltaInSeconds;
                return;
            }

            Point? spawnPoint = GetSpawnPoint();
            if (spawnPoint.HasValue)
            {
                Unit trainee = Unit.Faction.CreateUnit(traineeType, spawnPoint.Value);
                Vector2 traineeDelta = trainee.Center - Unit.Center;
                trainee.Angle = (float)Math.Atan2(traineeDelta.Y, traineeDelta.X);
                if (Unit.HasRallyPoint)
                {
                    MoveTask moveToRallyPointTask = new MoveTask(trainee, (Point)Unit.RallyPoint.Value);
                    trainee.TaskQueue.OverrideWith(moveToRallyPointTask);
                }

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

        private Point? GetSpawnPoint()
        {
            Region trainerRegion = Unit.GridRegion;

            Region spawnRegion = new Region(
                trainerRegion.MinX - traineeType.Size.Width,
                trainerRegion.MinY - traineeType.Size.Height,
                trainerRegion.Size.Width + traineeType.Size.Width,
                trainerRegion.Size.Height + traineeType.Size.Height);
            var potentialSpawnPoints = spawnRegion.InternalBorderPoints
                .Where(point =>
                    {
                        Region region = new Region(point, traineeType.Size);
                        return new Region(Unit.World.Size).Contains(region)
                            && Unit.World.IsFree(new Region(point, traineeType.Size), traineeType.CollisionLayer);
                    });

            if (Unit.HasRallyPoint)
            {
                return potentialSpawnPoints
                    .Select(point => (Point?)point)
                    .WithMinOrDefault(point => ((Vector2)point - Unit.RallyPoint.Value).LengthSquared);
            }
            else
            {
                return potentialSpawnPoints.FirstOrNull();
            }
        }
        #endregion
    }
}
