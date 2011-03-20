using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// An AI commander which is pretty much helpless.
    /// </summary>
    public sealed class HarvestingAICommander : Commander
    {
        #region Nested Types
        #region ResourceNodeData
        private sealed class ResourceNodeData
        {
            public readonly Entity Node;
            public int HarvesterCount;
            public Entity NearbyDepot;
            public Entity Extractor;

            public ResourceNodeData(Entity node)
            {
                Argument.EnsureNotNull(node, "node");
                this.Node = node;
            }
        }
        #endregion

        #region PlaceToExplore
        private sealed class PlaceToExplore
        {
            public readonly Point Point;
            public float TimeLastExplorerSent;
            public int ExplorationCount;

            public PlaceToExplore(Point point) { this.Point = point; }
        }
        #endregion
        #endregion

        #region Fields
        private const int explorationZoneSize = 8;
        private const float updatePeriod = 1.5f;
        private const int maximumHarvestersPerResourceNode = 4;
        private const float minimumBuildingDistance = 4;
        private const float maximumResourceDepotDistance = 12;
        private const float minimumPyramidDistance = 30;
        private const float minimumTimeBetweenSuccessiveExplorations = 10;
        private const int maximumExplorationCount = 6;

        private readonly Entity workerPrototype;
        private readonly Entity foodSupplyPrototype;
        private readonly Entity resourceDepotPrototype;
        private readonly Entity alageneExtractorPrototype;
        private readonly Entity pyramidPrototype;
        private readonly Entity defenseTowerPrototype;
        private readonly Entity laboratoryPrototype;

        private readonly List<PlaceToExplore> placesToExplore = new List<PlaceToExplore>();
        private readonly HashSet<Entity> createdPrototypes = new HashSet<Entity>();
        private readonly HashSet<Entity> buildings = new HashSet<Entity>();
        private readonly Dictionary<Entity, ResourceNodeData> resourceNodesData
            = new Dictionary<Entity, ResourceNodeData>();

        /// <summary>
        /// A set of <see cref="Entity">entities</see> which have been assigned tasks within the update.
        /// </summary>
        private readonly HashSet<Entity> assignedEntities = new HashSet<Entity>();

        private float lastUpdateTime;
        #endregion

        #region Constructors
        public HarvestingAICommander(Match match, Faction faction)
            : base(match, faction)
        {
            workerPrototype = match.UnitTypes.FromName("Smurf");
            foodSupplyPrototype = match.UnitTypes.FromName("Supply");
            resourceDepotPrototype = match.UnitTypes.FromName("HellsCostco");
            alageneExtractorPrototype = match.UnitTypes.FromName("AlageneExtractor");
            pyramidPrototype = match.UnitTypes.FromName("Pyramid");
            defenseTowerPrototype = match.UnitTypes.FromName("JeanMarc");
            laboratoryPrototype = match.UnitTypes.FromName("TristansHouse");

            for (int y = 0; y < World.Height / explorationZoneSize; ++y)
            {
                for (int x = 0; x < World.Width / explorationZoneSize; ++x)
                {
                    Point point = new Point(
                        x * explorationZoneSize + explorationZoneSize / 2,
                        y * explorationZoneSize + explorationZoneSize / 2);
                    placesToExplore.Add(new PlaceToExplore(point));
                }
            }

            lastUpdateTime = faction.Handle.Value / (float)match.World.Factions.Count() * updatePeriod;

            match.World.EntityAdded += OnEntityAdded;
            match.World.EntityRemoved += OnEntityRemoved;
        }
        #endregion

        #region Properties
        private Random Random
        {
            get { return Match.Random; }
        }

        private IEnumerable<Entity> VisibleResourceNodes
        {
            get
            {
                foreach (Entity entity in World.Entities)
                {
                    if (!entity.Components.Has<Harvestable>()) continue;
                    if (!Faction.CanSee(entity)) continue;
                    yield return entity;
                }
            }
        }

        private IEnumerable<Entity> IdleEntities
        {
            get
            {
                return Faction.Entities
                    .Where(entity => TaskQueue.HasEmpty(entity) && !assignedEntities.Contains(entity));
            }
        }
        #endregion

        #region Methods
        public override void Update(SimulationStep step)
        {
            if (step.TimeInSeconds - lastUpdateTime < updatePeriod)
                return;

            lastUpdateTime = step.TimeInSeconds;

            assignedEntities.Clear();
            ResourceAmount budget = new ResourceAmount(
                Faction.AladdiumAmount, Faction.AlageneAmount,
                Faction.RemainingFoodAmount);

            UpdateFoodSupplies(ref budget);
            UpdateDefense(ref budget);
            UpdateTechnologies(ref budget);
            UpdateResourceNodes(ref budget);
            UpdateExploration();

            foreach (Entity entity in IdleEntities)
            {
                UpdateIdleUnit(step, entity, ref budget);
            }
        }

        #region Specific updates
        private void UpdateFoodSupplies(ref ResourceAmount budget)
        {
            bool isLowOnFood = budget.Food / (float)Faction.MaxFoodAmount < 0.2f
                && Faction.MaxFoodAmount != World.MaximumFoodAmount;
            if (isLowOnFood)
            {
                var foodSupplyCost = GetCost(foodSupplyPrototype);
                if (budget >= foodSupplyCost)
                {
                    Entity entity = GetMostIdleUnit(u => Builder.Supports(u, foodSupplyPrototype));
                    if (entity != null && TryBuildNear(entity, foodSupplyPrototype, entity.Center))
                        assignedEntities.Add(entity);
                }

                budget -= foodSupplyCost;
            }
        }
        
        private void UpdateDefense(ref ResourceAmount budget)
        {
            var firstUnthreatenedVisibleEnemy = World.Entities.OfType<Entity>()
                .FirstOrDefault(u => Faction.CanSee(u)
                    && buildings.None(b => Identity.GetPrototype(b) == defenseTowerPrototype && (b.Center - u.Center).LengthFast < 5));
            if (firstUnthreatenedVisibleEnemy == null) return;
            
            var defenseTowerCost = GetCost(defenseTowerPrototype);
            if (budget >= defenseTowerCost)
            {
                Entity entity = GetNearbyUnit(u => Builder.Supports(u, defenseTowerPrototype), firstUnthreatenedVisibleEnemy.Center);
                if (entity != null && TryBuildNear(entity, defenseTowerPrototype, firstUnthreatenedVisibleEnemy.Center))
                    assignedEntities.Add(entity);
            }
            
            budget -= defenseTowerCost;
        }
        
        private void UpdateTechnologies(ref ResourceAmount budget)
        {
            if (Faction.UsedFoodAmount < 50) return;
            if (buildings.Any(b => Identity.GetPrototype(b) == laboratoryPrototype)) return;
            
            var laboratoryCost = GetCost(laboratoryPrototype);
            if (budget >= laboratoryCost)
            {
                Entity entity = GetMostIdleUnit(u => Builder.Supports(u, laboratoryPrototype));
                if (entity != null && TryBuildNear(entity, laboratoryPrototype, entity.Center))
                    assignedEntities.Add(entity);
            }
            
            budget -= laboratoryCost;
        }

        private void UpdateResourceNodes(ref ResourceAmount budget)
        {
            foreach (ResourceNodeData data in resourceNodesData.Values)
                data.HarvesterCount = 0;

            foreach (Entity entity in Faction.Entities)
            {
                TaskQueue taskQueue = entity.Components.TryGet<TaskQueue>();
                if (taskQueue == null) continue;

                HarvestTask harvestTask = taskQueue.FirstOrDefault() as HarvestTask;
                if (harvestTask == null) continue;

                ResourceNodeData nodeData;
                if (!resourceNodesData.TryGetValue(harvestTask.ResourceNode, out nodeData))
                {
                    // This can happen when a node depletes.
                    continue;
                }

                ++nodeData.HarvesterCount;
            }

            var resourceNodes = World.Entities.Where(e => e.Components.Has<Harvestable>());
            foreach (Entity node in resourceNodes)
            {
                ResourceNodeData nodeData;
                if (!resourceNodesData.TryGetValue(node, out nodeData))
                {
                    if (!Faction.CanSee(node)) continue;

                    nodeData = new ResourceNodeData(node);
                    resourceNodesData.Add(node, nodeData);
                }


                if (nodeData.Node.Components.Get<Harvestable>().Type == ResourceType.Alagene
                    && (nodeData.Extractor == null || !nodeData.Extractor.IsAliveInWorld)
                    && World.IsFree(node.GridRegion, CollisionLayer.Ground))
                {
                    var alageneExtractorCost = GetCost(alageneExtractorPrototype);
                    if (budget >= alageneExtractorCost)
                    {
                        Entity builder = GetNearbyUnit(u => Builder.Supports(u, alageneExtractorPrototype), node.Center);
                        if (builder != null)
                        {
                            var command = new BuildCommand(Faction.Handle, builder.Handle, alageneExtractorPrototype.Handle, Point.Truncate(node.Position));
                            IssueCommand(command);
                            assignedEntities.Add(builder);
                        }
                    }

                    budget -= alageneExtractorCost;
                }
                
                if (nodeData.HarvesterCount == 0 || (nodeData.NearbyDepot != null && nodeData.NearbyDepot.IsAliveInWorld))
                    continue;

                // Find a nearby depot;
                nodeData.NearbyDepot = World.Entities
                    .Intersecting(new Circle(node.Center, maximumResourceDepotDistance))
                    .OfType<Entity>()
                    .Where(u => FactionMembership.GetFaction(u) == Faction && u.Components.Has<ResourceDepot>())
                    .WithMinOrDefault(u => (u.Center - node.Center).LengthSquared);
                if (nodeData.NearbyDepot != null) continue;
                
                // Build a nearby depot or pyramid
                Entity buildingType = resourceDepotPrototype;

                Entity nearestPyramid = buildings
                    .Where(building => Identity.GetPrototype(building) == pyramidPrototype)
                    .WithMinOrDefault(pyramid => (pyramid.Center - node.Center).LengthSquared);
                if (nearestPyramid == null || (nearestPyramid.Center - node.Center).LengthFast > minimumPyramidDistance)
                    buildingType = pyramidPrototype;

                var buildingCost = GetCost(buildingType);
                if (budget >= buildingCost)
                {
                    Entity builder = GetNearbyUnit(u => Builder.Supports(u, buildingType), node.Center);
                    if (builder != null && TryBuildNear(builder, buildingType, node.Center))
                        assignedEntities.Add(builder);
                }

                budget -= buildingCost;
            }
        }

        private void UpdateExploration()
        {
            placesToExplore.RemoveAll(place => Faction.GetTileVisibility(place.Point) != TileVisibility.Undiscovered);
        }

        private void UpdateIdleUnit(SimulationStep step, Entity entity, ref ResourceAmount budget)
        {
            if (entity.Components.Has<BuildProgress>()) return;

            Trainer trainer = entity.Components.TryGet<Trainer>();
            if (trainer != null && trainer.Supports(workerPrototype))
            {
                var workerCost = GetCost(workerPrototype);
                int workerToCreateCount = budget.GetQuotient(workerCost);
                if (workerToCreateCount > 0)
                {
                    var command = new TrainCommand(Faction.Handle, entity.Handle, workerPrototype.Handle, workerToCreateCount);
                    IssueCommand(command);
                    budget -= workerCost * workerToCreateCount;
                    return;
                }
            }

            if (entity.Components.Has<Harvester>())
            {
                var nodeData = resourceNodesData.Values
                    .Where(d => Faction.CanHarvest(d.Node) && d.HarvesterCount < maximumHarvestersPerResourceNode)
                    .WithMinOrDefault(d => Region.Distance(entity.GridRegion, d.Node.GridRegion) + d.HarvesterCount * 2);
                if (nodeData != null)
                {
                    ++nodeData.HarvesterCount;
                    var command = new HarvestCommand(Faction.Handle,
                        entity.Handle, nodeData.Node.Handle);
                    IssueCommand(command);
                    return;
                }
            }

            if (entity.Components.Has<Mobile>())
            {
                var placeToExplore = placesToExplore
                    .Where(p => step.TimeInSeconds - p.TimeLastExplorerSent > minimumTimeBetweenSuccessiveExplorations)
                    .WithMinOrDefault(p => (entity.Center - p.Point).LengthSquared);
                if (placeToExplore != null)
                {
                    placeToExplore.TimeLastExplorerSent = step.TimeInSeconds;
                    ++placeToExplore.ExplorationCount;
                    if (placeToExplore.ExplorationCount >= maximumExplorationCount)
                        placesToExplore.Remove(placeToExplore);

                    var command = new MoveCommand(Faction.Handle, entity.Handle, placeToExplore.Point);
                    IssueCommand(command);

                    return;
                }
            }

            Researcher researcher = entity.Components.TryGet<Researcher>();
            if (researcher != null)
            {
                foreach (var technology in Match.TechnologyTree.Technologies)
                {
                    if (!researcher.Supports(technology)) continue;
                    if (Faction.HasResearched(technology)) continue;
                    if (createdPrototypes.None(type => technology.AppliesTo(type))) continue;
                    
                    var cost = new ResourceAmount(technology.AladdiumCost, technology.AlageneCost);
                    if (!(budget >= cost)) continue;
                    
                    var command = new ResearchCommand(Faction.Handle, entity.Handle, technology.Handle);
                    IssueCommand(command);
                    
                    budget -= cost;
                    
                    break;
                }
            }

            foreach (UnitTypeUpgrade upgrade in entity.Identity.Upgrades)
            {
                if (upgrade.IsFree) continue;
                
                var upgradeCost = new ResourceAmount(upgrade.AladdiumCost, upgrade.AlageneCost);
                if (!(budget >= upgradeCost)) continue;
                
                var upgradedPrototype = Match.UnitTypes.FromName(upgrade.Target);
                if (upgradedPrototype == null) continue;
                
                var command = new UpgradeCommand(Faction.Handle, entity.Handle, upgradedPrototype.Handle);
                IssueCommand(command);
                
                budget -= upgradeCost;
                break;
            }
        }
        #endregion

        #region Helpers
        private ResourceAmount GetCost(Entity prototype)
        {
            return ResourceAmount.FromEntityCost(prototype, Faction);
        }

        private Entity GetNearbyUnit(Func<Entity, bool> predicate, Vector2 location)
        {
            return Faction.Entities
                .Where(entity => !assignedEntities.Contains(entity) && predicate(entity))
                .WithMinOrDefault(entity => (entity.Center - location).LengthFast + GetOccupationImportance(entity));
        }

        private Entity GetMostIdleUnit(Func<Entity, bool> predicate)
        {
            return Faction.Entities
                .Where(entity => !assignedEntities.Contains(entity) && predicate(entity))
                .WithMinOrDefault(entity => GetOccupationImportance(entity));
        }

        private float GetOccupationImportance(Entity entity)
        {
            Task task = entity.Components.Get<TaskQueue>().FirstOrDefault();
            if (task == null || task is MoveTask) return 0;
            if (task is HarvestTask) return 4;
            if (task is BuildTask) return 30;
            return 2;
        }

        private bool TryBuildNear(Entity builder, Entity buildingType, Vector2 location)
        {
            Point buildingLocation = new Point(
                            (int)location.X + Random.Next(-8, 9),
                            (int)location.Y + Random.Next(-8, 9));
            buildingLocation = new Region(
                World.Width - foodSupplyPrototype.Size.Width,
                World.Height - foodSupplyPrototype.Size.Height)
                .Clamp(buildingLocation);

            Region buildingRegion = new Region(buildingLocation, buildingType.Size);
            if (!Faction.CanSee(buildingRegion) || !World.IsFree(buildingRegion, CollisionLayer.Ground))
                return false;

            bool isNearOtherBuilding = buildings
                .Any(b => Region.Distance(b.GridRegion, buildingRegion) < minimumBuildingDistance);
            if (isNearOtherBuilding) return false;
            
            var command = new BuildCommand(Faction.Handle, builder.Handle, buildingType.Handle, buildingLocation);
            IssueCommand(command);
            return true;
        }
        #endregion

        #region Event Handlers
        private void OnEntityAdded(World sender, Entity entity)
        {
            if (FactionMembership.GetFaction(entity) != Faction) return;

            var prototype = Identity.GetPrototype(entity);
            createdPrototypes.Add(prototype);
            
            if (entity.Identity.IsBuilding)
            {
                buildings.Add(entity);
                if (prototype == alageneExtractorPrototype)
                {
                    Entity node = World.Entities.Intersecting(entity.Center)
                        .First(e => e.Components.Has<Harvestable>());

                    var nodeData = resourceNodesData[node];
                    nodeData.Extractor = entity;
                }
            }
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            if (!entity.Components.Has<Harvestable>()) return;

            if (entity != null)
            {
                resourceNodesData.Remove(entity);
                return;
            }

            if (FactionMembership.GetFaction(entity) != Faction) return;

            buildings.Remove((Entity)entity);
        }
        #endregion
        #endregion
    }
}
