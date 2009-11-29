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
                Point? spawnPoint = GetSpawnPoint();
                if (spawnPoint.HasValue)
                {
                    Unit trainee = trainer.Faction.CreateUnit(traineeType, spawnPoint.Value);
                    Vector2 traineeDelta = trainee.Center - trainer.Center;
                    trainee.Angle = (float)Math.Atan2(traineeDelta.Y, traineeDelta.X);
                    if (trainer.HasRallyPoint) trainee.CurrentTask = new Move(trainee, trainer.RallyPoint.Value);
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
            if (traineeType.IsAirborne) return (Point)trainer.Center;

            var adjacentPoints = trainer.GridRegion.GetAdjacentPoints()
                    .Where(point => trainer.World.IsWithinBounds(point) && trainer.World.IsFree(point));
            
            if (trainer.HasRallyPoint)
            {
                return adjacentPoints
                    .Select(point => (Point?)point)
                    .WithMinOrDefault(point => ((Vector2)point - trainer.RallyPoint.Value).LengthSquared);
            }
            else
            {
                return adjacentPoints.FirstOrNull();
            }
        }
        #endregion
    }
}
