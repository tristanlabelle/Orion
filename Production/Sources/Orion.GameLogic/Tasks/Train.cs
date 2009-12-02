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
    public sealed class Train : Task
    {
        #region Fields
        private readonly UnitType traineeType;
        private float healthPointsTrained = 0;
        private bool hasEnded = false;
        #endregion

        #region Constructors
        public Train(Unit trainer, UnitType traineeType)
            : base(trainer)
        {
            Argument.EnsureNotNull(trainer, "trainer");
            if (!trainer.HasSkill<Skills.Train>())
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
        protected override void DoUpdate(float timeDelta)
        {
            if (Unit.Faction.RemainingFoodAmount < traineeType.FoodCost) return;

            float maxHealth = Unit.Faction.GetStat(traineeType, UnitStat.MaxHealth);
            float trainingSpeed = Unit.GetStat(UnitStat.TrainingSpeed);
            healthPointsTrained += trainingSpeed * timeDelta;
            if (healthPointsTrained >= maxHealth)
            {
                Point? spawnPoint = GetSpawnPoint();
                if (spawnPoint.HasValue)
                {
                    Unit trainee = Unit.Faction.CreateUnit(traineeType, spawnPoint.Value);
                    Vector2 traineeDelta = trainee.Center - Unit.Center;
                    trainee.Angle = (float)Math.Atan2(traineeDelta.Y, traineeDelta.X);
                    if (Unit.HasRallyPoint) trainee.CurrentTask = Move.ToPoint(trainee, Unit.RallyPoint.Value);
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
            if (traineeType.IsAirborne) return (Point)Unit.Center;

            var adjacentPoints = Unit.GridRegion.GetAdjacentPoints()
                    .Where(point => Unit.World.IsWithinBounds(point) && Unit.World.IsFree(point));

            if (Unit.HasRallyPoint)
            {
                return adjacentPoints
                    .Select(point => (Point?)point)
                    .WithMinOrDefault(point => ((Vector2)point - Unit.RallyPoint.Value).LengthSquared);
            }
            else
            {
                return adjacentPoints.FirstOrNull();
            }
        }
        #endregion
    }
}
