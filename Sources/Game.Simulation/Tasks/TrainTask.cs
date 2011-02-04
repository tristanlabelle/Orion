using System;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A task which causes a unit (typically a building) to create a unit.
    /// </summary>
    [Serializable]
    public sealed class TrainTask : Task
    {
        #region Fields
        private readonly Unit traineeType;
        private float healthPointsTrained;
        private bool attemptingToPlaceUnit;
        private bool waitingForEnoughFood;
        #endregion

        #region Constructors
        public TrainTask(Unit trainer, Unit traineeType)
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

        public Unit TraineeType
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
                    string warning = "Pas assez de nourriture pour entraîner un {0}".FormatInvariant(traineeType.Name);
                    Faction.RaiseWarning(warning);
                }

                return;
            }
            waitingForEnoughFood = false;

            int maxHealth = Unit.Faction.GetStat(traineeType, BasicSkill.MaxHealthStat);
            if (healthPointsTrained < maxHealth)
            {
                float trainingSpeed = Unit.GetStat(TrainSkill.SpeedStat);
                healthPointsTrained += trainingSpeed * step.TimeDeltaInSeconds;
                return;
            }

            Unit trainee = TrySpawn(traineeType);
            if (trainee == null)
            {
                if (!attemptingToPlaceUnit)
                {
                    attemptingToPlaceUnit = true;
                    string warning = "Pas de place pour faire apparaître un {0}".FormatInvariant(traineeType.Name);
                    Faction.RaiseWarning(warning);
                }
                return;
            }

            TryApplyRallyPoint(trainee);

            MarkAsEnded();
        }

        private Point? TryGetFreeSurroundingSpawnPoint(Unit spawneeType)
        {
            Argument.EnsureNotNull(spawneeType, "spawneeType");

            Region trainerRegion = Unit.GridRegion;

            Region spawnRegion = new Region(
                trainerRegion.MinX - spawneeType.Size.Width,
                trainerRegion.MinY - spawneeType.Size.Height,
                trainerRegion.Size.Width + spawneeType.Size.Width,
                trainerRegion.Size.Height + spawneeType.Size.Height);
            var potentialSpawnPoints = spawnRegion.InternalBorderPoints
                .Where(point =>
                {
                    Region region = new Region(point, spawneeType.Size);
                    CollisionLayer layer = spawneeType.GetComponent<Spatial>().CollisionLayer;
                    return new Region(World.Size).Contains(region)
                        && World.IsFree(new Region(point, spawneeType.Size), layer);
                });

            if (Unit.HasRallyPoint)
            {
                return potentialSpawnPoints
                    .Select(point => (Point?)point)
                    .WithMinOrDefault(point => ((Vector2)point - Unit.RallyPoint).LengthSquared);
            }
            else
            {
                return potentialSpawnPoints.FirstOrNull();
            }
        }

        private Unit TrySpawn(Unit spawneeType)
        {
            Argument.EnsureNotNull(spawneeType, "spawneeType");

            Point? point = TryGetFreeSurroundingSpawnPoint(spawneeType);
            if (!point.HasValue) return null;

            Unit spawnee = Unit.Faction.CreateUnit(spawneeType, point.Value);
            Vector2 traineeDelta = spawnee.Center - Unit.Center;
            spawnee.Angle = (float)Math.Atan2(traineeDelta.Y, traineeDelta.X);

            return spawnee;
        }

        private void TryApplyRallyPoint(Unit unit)
        {
            if (!Unit.HasRallyPoint) return;

            Task rallyPointTask = null;
            // Check to see if we can harvest automatically
            if (unit.HasSkill<HarvestSkill>())
            {
                Entity resourceNode = World.Entities
                    .Intersecting(Unit.RallyPoint)
                    .Where(e => e.HasComponent<Harvestable>())
                    .FirstOrDefault(e => !e.GetComponent<Harvestable>().IsEmpty);

                if (resourceNode != null && unit.Faction.CanHarvest(resourceNode))
                    rallyPointTask = new HarvestTask(unit, resourceNode);
            }
            
            if (rallyPointTask == null)
            {
                Point targetPoint = (Point)Unit.RallyPoint;
                rallyPointTask = new MoveTask(unit, targetPoint);
            }

            unit.TaskQueue.OverrideWith(rallyPointTask);
        }
        #endregion
    }
}
