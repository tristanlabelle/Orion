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
        public TrainTask(Entity trainer, Unit traineeType)
            : base(trainer)
        {
            Argument.EnsureNotNull(trainer, "trainer");
            Argument.EnsureNotNull(traineeType, "traineeType");
            Argument.EnsureEqual(traineeType.IsBuilding, false, "traineeType.IsBuilding");
            if (((Unit)trainer).IsUnderConstruction)
                throw new ArgumentException("Cannot train with an building under construction");

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
        
        public override float Progress
        {
            get
            {
                int maxHealth = FactionMembership.GetFaction(Entity).GetStat(traineeType, BasicSkill.MaxHealthStat);
                return Math.Min(healthPointsTrained / maxHealth, 1);
            }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (Entity.Spatial == null
                || !Entity.Components.Has<Trainer>()
                || FactionMembership.GetFaction(Entity) == null)
            {
                MarkAsEnded();
                return;
            }

            Faction faction = FactionMembership.GetFaction(Entity);
            if (faction != null && faction.RemainingFoodAmount < faction.GetStat(traineeType, BasicSkill.FoodCostStat))
            {
                if (!waitingForEnoughFood)
                {
                    waitingForEnoughFood = true;
                    string warning = "Pas assez de nourriture pour entraîner un {0}".FormatInvariant(traineeType.Name);
                    faction.RaiseWarning(warning);
                }

                return;
            }
            waitingForEnoughFood = false;

            int maxHealth = faction.GetStat(traineeType, BasicSkill.MaxHealthStat);
            if (healthPointsTrained < maxHealth)
            {
                float trainingSpeed = (float)Entity.GetStatValue(Trainer.SpeedStat);
                healthPointsTrained += trainingSpeed * step.TimeDeltaInSeconds;
                return;
            }

            Unit trainee = TrySpawn(traineeType);
            if (trainee == null)
            {
                if (faction != null && !attemptingToPlaceUnit)
                {
                    attemptingToPlaceUnit = true;
                    string warning = "Pas de place pour faire apparaître un {0}".FormatInvariant(traineeType.Name);
                    faction.RaiseWarning(warning);
                }
                return;
            }

            TryApplyRallyPoint(trainee);

            MarkAsEnded();
        }

        private Point? TryGetFreeSurroundingSpawnPoint(Unit spawneeType)
        {
            Argument.EnsureNotNull(spawneeType, "spawneeType");

            Region trainerRegion = Entity.Spatial.GridRegion;

            Region spawnRegion = new Region(
                trainerRegion.MinX - spawneeType.Size.Width,
                trainerRegion.MinY - spawneeType.Size.Height,
                trainerRegion.Size.Width + spawneeType.Size.Width,
                trainerRegion.Size.Height + spawneeType.Size.Height);
            var potentialSpawnPoints = spawnRegion.InternalBorderPoints
                .Where(point =>
                {
                    Region region = new Region(point, spawneeType.Size);
                    CollisionLayer layer = spawneeType.Spatial.CollisionLayer;
                    return new Region(World.Size).Contains(region)
                        && World.IsFree(new Region(point, spawneeType.Size), layer);
                });

            Trainer trainer = Entity.Components.Get<Trainer>();
            if (trainer.HasRallyPoint)
            {
                return potentialSpawnPoints
                    .Select(point => (Point?)point)
                    .WithMinOrDefault(point => ((Vector2)point - trainer.RallyPoint.Value).LengthSquared);
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

            Unit spawnee = FactionMembership.GetFaction(Entity).CreateUnit(spawneeType, point.Value);
            Vector2 traineeDelta = spawnee.Center - Entity.Spatial.Center;
            spawnee.Spatial.Angle = (float)Math.Atan2(traineeDelta.Y, traineeDelta.X);

            return spawnee;
        }

        private void TryApplyRallyPoint(Entity entity)
        {
            Trainer trainer = Entity.Components.Get<Trainer>();
            if (!trainer.HasRallyPoint) return;

            Vector2 rallyPoint = trainer.RallyPoint.Value;

            Task rallyPointTask = null;
            // Check to see if we can harvest automatically
            Harvester harvester = entity.Components.TryGet<Harvester>();
            if (harvester != null)
            {
                Entity resourceNode = World.Entities
                    .Intersecting(rallyPoint)
                    .Where(e => e.Components.Has<Harvestable>())
                    .FirstOrDefault(e => !e.Components.Get<Harvestable>().IsEmpty);

                if (resourceNode != null && harvester.CanHarvest(resourceNode))
                    rallyPointTask = new HarvestTask(entity, resourceNode);
            }
            
            if (rallyPointTask == null)
            {
                Point targetPoint = (Point)rallyPoint;
                rallyPointTask = new MoveTask(entity, targetPoint);
            }

            entity.Components.Get<TaskQueue>().OverrideWith(rallyPointTask);
        }
        #endregion
    }
}
