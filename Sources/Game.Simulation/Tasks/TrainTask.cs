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
    /// A task which causes an <see cref="Entity"/> (typically a building) to create another <see cref="Entity"/>.
    /// </summary>
    [Serializable]
    public sealed class TrainTask : Task
    {
        #region Fields
        private readonly Entity prototype;
        private float elapsedTime;
        private bool attemptingToPlaceEntity;
        private bool waitingForEnoughFood;
        #endregion

        #region Constructors
        public TrainTask(Entity trainer, Entity prototype)
            : base(trainer)
        {
            Argument.EnsureNotNull(trainer, "trainer");
            Argument.EnsureNotNull(prototype, "prototype");

            this.prototype = prototype;
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Training"; }
        }

        public Entity Prototype
        {
            get { return prototype; }
        }
        
        public override float Progress
        {
            get
            {
                float requiredTime = (float)FactionMembership.GetFaction(Entity)
                    .GetStat(prototype, Identity.SpawnTimeStat);
                return Math.Min(elapsedTime / requiredTime, 1);
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
            int foodCost = (int)faction.GetStat(prototype, FactionMembership.FoodCostStat);
            if (faction != null && faction.RemainingFoodAmount < foodCost)
            {
                if (!waitingForEnoughFood)
                {
                    waitingForEnoughFood = true;
                    string warning = "Pas assez de nourriture pour entraîner un {0}"
                        .FormatInvariant(prototype.Identity.Name);
                    faction.RaiseWarning(warning);
                }

                return;
            }
            waitingForEnoughFood = false;

            float trainingSpeed = (float)Entity.GetStatValue(Trainer.SpeedStat);
            elapsedTime += step.TimeDeltaInSeconds * trainingSpeed;

            float requiredTime = (float)faction.GetStat(prototype, Identity.SpawnTimeStat);
            if (elapsedTime < requiredTime) return;

            Entity trainee = TrySpawn();
            if (trainee == null)
            {
                if (faction != null && !attemptingToPlaceEntity)
                {
                    attemptingToPlaceEntity = true;
                    string warning = "Pas de place pour faire apparaître un {0}"
                        .FormatInvariant(prototype.Identity.Name);
                    faction.RaiseWarning(warning);
                }
                return;
            }

            TryApplyRallyPoint(trainee);

            MarkAsEnded();
        }

        private Point? TryGetFreeSurroundingSpawnPoint(Entity spawneePrototype)
        {
            Argument.EnsureNotNull(spawneePrototype, "spawneePrototype");

            Region trainerRegion = Entity.Spatial.GridRegion;

            Region spawnRegion = new Region(
                trainerRegion.MinX - spawneePrototype.Size.Width,
                trainerRegion.MinY - spawneePrototype.Size.Height,
                trainerRegion.Size.Width + spawneePrototype.Size.Width,
                trainerRegion.Size.Height + spawneePrototype.Size.Height);
            var potentialSpawnPoints = spawnRegion.InternalBorderPoints
                .Where(point =>
                {
                    Region region = new Region(point, spawneePrototype.Size);
                    CollisionLayer layer = spawneePrototype.Spatial.CollisionLayer;
                    return new Region(World.Size).Contains(region)
                        && World.IsFree(new Region(point, spawneePrototype.Size), layer);
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

        private Entity TrySpawn()
        {
            Point? point = TryGetFreeSurroundingSpawnPoint(prototype);
            if (!point.HasValue) return null;

            Entity spawnee = FactionMembership.GetFaction(Entity).CreateUnit(prototype, point.Value);
            Vector2 traineeDelta = spawnee.Spatial.Center - Entity.Spatial.Center;
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
