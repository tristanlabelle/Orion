using System;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using System.Diagnostics;

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
        private float healthPointsTrained = 0;
        private bool hasEnded = false;
        #endregion

        #region Constructors
        public TrainTask(Unit trainer, UnitType traineeType)
            : base(trainer)
        {
            Argument.EnsureNotNull(trainer, "trainer");
            if (!trainer.HasSkill<Skills.TrainSkill>())
                throw new ArgumentException("Cannot train without the train skill.", "trainer");
            if (trainer.IsUnderConstruction)
                throw new ArgumentException("Cannot train with an Unit in Construction");
            Argument.EnsureNotNull(traineeType, "traineeType");
            Argument.EnsureEqual(traineeType.IsBuilding, false, "traineeType.IsBuilding");

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
                int maxHealth = Unit.Faction.GetStat(traineeType, UnitStat.MaxHealth);
                return healthPointsTrained / maxHealth;
            }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationUpdateInfo info)
        {
            if (Unit.Faction.RemainingFoodAmount < traineeType.FoodCost) return;

            float maxHealth = Unit.Faction.GetStat(traineeType, UnitStat.MaxHealth);
            float trainingSpeed = Unit.GetStat(UnitStat.TrainingSpeed);
            healthPointsTrained += trainingSpeed * info.TimeDeltaInSeconds;
            if (healthPointsTrained >= maxHealth)
            {
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
                    hasEnded = true;
                }
                else
                {
                    Debug.WriteLine("No place to spawn a unit.");
                }
            }
        }

        private Point? GetSpawnPoint()
        {
            Region trainerRegion = Unit.GridRegion;
            Region spawnRegion = new Region(
                trainerRegion.MinX - traineeType.Size.Width,
                trainerRegion.MinY - traineeType.Size.Height,
                trainerRegion.Size.Width + traineeType.Size.Width + 1,
                trainerRegion.Size.Height + traineeType.Size.Height + 1);

            var potentialSpawnPoints = spawnRegion.Points
                .Where(point => Unit.World.IsFree(new Region(point, traineeType.Size), traineeType.CollisionLayer));

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
