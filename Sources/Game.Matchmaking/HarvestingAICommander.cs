using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components;
using System.Diagnostics;

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
            public Unit NearbyDepot;
            public Unit Extractor;

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

        private readonly Unit workerUnitType;
        private readonly Unit foodSupplyUnitType;
        private readonly Unit resourceDepotUnitType;
        private readonly Unit alageneExtractorUnitType;
        private readonly Unit pyramidUnitType;
        private readonly Unit defenseTowerUnitType;
        private readonly Unit laboratoryUnitType;

        private readonly List<PlaceToExplore> placesToExplore = new List<PlaceToExplore>();
        private readonly HashSet<Unit> createdUnitTypes = new HashSet<Unit>();
        private readonly HashSet<Unit> buildings = new HashSet<Unit>();
        private readonly Dictionary<Entity, ResourceNodeData> resourceNodesData
            = new Dictionary<Entity, ResourceNodeData>();

        /// <summary>
        /// A set of units which have been assigned tasks within the update.
        /// </summary>
        private readonly HashSet<Unit> assignedUnits = new HashSet<Unit>();

        private float lastUpdateTime;
        #endregion

        #region Constructors
        public HarvestingAICommander(Match match, Faction faction)
            : base(match, faction)
        {
            workerUnitType = match.UnitTypes.FromName("Smurf");
            foodSupplyUnitType = match.UnitTypes.FromName("Supply");
            resourceDepotUnitType = match.UnitTypes.FromName("HellsCostco");
            alageneExtractorUnitType = match.UnitTypes.FromName("AlageneExtractor");
            pyramidUnitType = match.UnitTypes.FromName("Pyramid");
            defenseTowerUnitType = match.UnitTypes.FromName("JeanMarc");
            laboratoryUnitType = match.UnitTypes.FromName("TristansHouse");

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

        private IEnumerable<Unit> IdleUnits
        {
            get
            {
                return Faction.Units
                    .Where(unit => TaskQueue.HasEmpty(unit) && !assignedUnits.Contains(unit));
            }
        }
        #endregion

        #region Methods
        public override void Update(SimulationStep step)
        {
            if (step.TimeInSeconds - lastUpdateTime < updatePeriod)
                return;

            lastUpdateTime = step.TimeInSeconds;

            assignedUnits.Clear();
            ResourceAmount budget = new ResourceAmount(
                Faction.AladdiumAmount, Faction.AlageneAmount,
                Faction.RemainingFoodAmount);

            UpdateFoodSupplies(ref budget);
            UpdateDefense(ref budget);
            UpdateTechnologies(ref budget);
            UpdateResourceNodes(ref budget);
            UpdateExploration();

            foreach (Unit unit in IdleUnits)
            {
                UpdateIdleUnit(step, unit, ref budget);
            }
        }

        #region Specific updates
        private void UpdateFoodSupplies(ref ResourceAmount budget)
        {
            bool isLowOnFood = budget.Food / (float)Faction.MaxFoodAmount < 0.2f
                && Faction.MaxFoodAmount != World.MaximumFoodAmount;
            if (isLowOnFood)
            {
                var foodSupplyCost = GetCost(foodSupplyUnitType);
                if (budget >= foodSupplyCost)
                {
                    Unit unit = GetMostIdleUnit(u => Builder.Supports(u, foodSupplyUnitType));
                    if (unit != null && TryBuildNear(unit, foodSupplyUnitType, unit.Center))
                        assignedUnits.Add(unit);
                }

                budget -= foodSupplyCost;
            }
        }
        
        private void UpdateDefense(ref ResourceAmount budget)
        {
#warning Unit type comparision
            var firstUnthreatenedVisibleEnemy = World.Entities.OfType<Unit>()
                .FirstOrDefault(u => Faction.CanSee(u)
                    && buildings.None(b => b.Type == defenseTowerUnitType && (b.Center - u.Center).LengthFast < 5));
            if (firstUnthreatenedVisibleEnemy == null) return;
            
            var defenseTowerCost = GetCost(defenseTowerUnitType);
            if (budget >= defenseTowerCost)
            {
                Unit unit = GetNearbyUnit(u => Builder.Supports(u, defenseTowerUnitType), firstUnthreatenedVisibleEnemy.Center);
                if (unit != null && TryBuildNear(unit, defenseTowerUnitType, firstUnthreatenedVisibleEnemy.Center))
                    assignedUnits.Add(unit);
            }
            
            budget -= defenseTowerCost;
        }
        
        private void UpdateTechnologies(ref ResourceAmount budget)
        {
            if (Faction.UsedFoodAmount < 50) return;
#warning Unit type comparision
            if (buildings.Any(b => b.Type == laboratoryUnitType)) return;
            
            var laboratoryCost = GetCost(laboratoryUnitType);
            if (budget >= laboratoryCost)
            {
                Unit unit = GetMostIdleUnit(u => Builder.Supports(u, laboratoryUnitType));
                if (unit != null && TryBuildNear(unit, laboratoryUnitType, unit.Center))
                    assignedUnits.Add(unit);
            }
            
            budget -= laboratoryCost;
        }

        private void UpdateResourceNodes(ref ResourceAmount budget)
        {
            foreach (ResourceNodeData data in resourceNodesData.Values)
                data.HarvesterCount = 0;

            foreach (Entity entity in Faction.Units)
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
                    var alageneExtractorCost = GetCost(alageneExtractorUnitType);
                    if (budget >= alageneExtractorCost)
                    {
                        Unit builder = GetNearbyUnit(u => Builder.Supports(u, alageneExtractorUnitType), node.Center);
                        if (builder != null)
                        {
                            var command = new BuildCommand(Faction.Handle, builder.Handle, alageneExtractorUnitType.Handle, Point.Truncate(node.Position));
                            IssueCommand(command);
                            assignedUnits.Add(builder);
                        }
                    }

                    budget -= alageneExtractorCost;
                }
                
                if (nodeData.HarvesterCount == 0 || (nodeData.NearbyDepot != null && nodeData.NearbyDepot.IsAliveInWorld))
                    continue;

                // Find a nearby depot;
                nodeData.NearbyDepot = World.Entities
                    .Intersecting(new Circle(node.Center, maximumResourceDepotDistance))
                    .OfType<Unit>()
                    .Where(u => FactionMembership.GetFaction(u) == Faction && u.Components.Has<ResourceDepot>())
                    .WithMinOrDefault(u => (u.Center - node.Center).LengthSquared);
                if (nodeData.NearbyDepot != null) continue;
                
                // Build a nearby depot or pyramid
                Unit buildingType = resourceDepotUnitType;

#warning Unit type comparision
                Unit nearestPyramid = buildings
                    .Where(building => building.Type == pyramidUnitType)
                    .WithMinOrDefault(pyramid => (pyramid.Center - node.Center).LengthSquared);
                if (nearestPyramid == null || (nearestPyramid.Center - node.Center).LengthFast > minimumPyramidDistance)
                    buildingType = pyramidUnitType;

                var buildingCost = GetCost(buildingType);
                if (budget >= buildingCost)
                {
                    Unit builder = GetNearbyUnit(u => Builder.Supports(u, buildingType), node.Center);
                    if (builder != null && TryBuildNear(builder, buildingType, node.Center))
                        assignedUnits.Add(builder);
                }

                budget -= buildingCost;
            }
        }

        private void UpdateExploration()
        {
            placesToExplore.RemoveAll(place => Faction.GetTileVisibility(place.Point) != TileVisibility.Undiscovered);
        }

        private void UpdateIdleUnit(SimulationStep step, Unit unit, ref ResourceAmount budget)
        {
            if (unit.Components.Has<BuildProgress>()) return;

            Trainer trainer = unit.Components.TryGet<Trainer>();
            if (trainer != null && trainer.Supports(workerUnitType))
            {
                var workerCost = GetCost(workerUnitType);
                int workerToCreateCount = budget.GetQuotient(workerCost);
                if (workerToCreateCount > 0)
                {
                    var command = new TrainCommand(Faction.Handle, unit.Handle, workerUnitType.Handle, workerToCreateCount);
                    IssueCommand(command);
                    budget -= workerCost * workerToCreateCount;
                    return;
                }
            }

            if (unit.Components.Has<Harvester>())
            {
                var nodeData = resourceNodesData.Values
                    .Where(d => Faction.CanHarvest(d.Node) && d.HarvesterCount < maximumHarvestersPerResourceNode)
                    .WithMinOrDefault(d => Region.Distance(unit.GridRegion, d.Node.GridRegion) + d.HarvesterCount * 2);
                if (nodeData != null)
                {
                    ++nodeData.HarvesterCount;
                    var command = new HarvestCommand(Faction.Handle,
                        unit.Handle, nodeData.Node.Handle);
                    IssueCommand(command);
                    return;
                }
            }

            if (unit.Components.Has<Move>())
            {
                var placeToExplore = placesToExplore
                    .Where(p => step.TimeInSeconds - p.TimeLastExplorerSent > minimumTimeBetweenSuccessiveExplorations)
                    .WithMinOrDefault(p => (unit.Center - p.Point).LengthSquared);
                if (placeToExplore != null)
                {
                    placeToExplore.TimeLastExplorerSent = step.TimeInSeconds;
                    ++placeToExplore.ExplorationCount;
                    if (placeToExplore.ExplorationCount >= maximumExplorationCount)
                        placesToExplore.Remove(placeToExplore);

                    var command = new MoveCommand(Faction.Handle, unit.Handle, placeToExplore.Point);
                    IssueCommand(command);

                    return;
                }
            }

            Researcher researcher = unit.Components.TryGet<Researcher>();
            if (researcher != null)
            {
                foreach (var technology in Match.TechnologyTree.Technologies)
                {
                    if (!researcher.Supports(technology)) continue;
                    if (Faction.HasResearched(technology)) continue;
                    if (createdUnitTypes.None(type => technology.AppliesTo(type))) continue;
                    
                    var cost = new ResourceAmount(technology.AladdiumCost, technology.AlageneCost);
                    if (!(budget >= cost)) continue;
                    
                    var command = new ResearchCommand(Faction.Handle, unit.Handle, technology.Handle);
                    IssueCommand(command);
                    
                    budget -= cost;
                    
                    break;
                }
            }
            
            foreach (UnitTypeUpgrade upgrade in unit.Upgrades)
            {
                if (upgrade.IsFree) continue;
                
                var upgradeCost = new ResourceAmount(upgrade.AladdiumCost, upgrade.AlageneCost);
                if (!(budget >= upgradeCost)) continue;
                
                var upgradedUnitType = Match.UnitTypes.FromName(upgrade.Target);
                if (upgradedUnitType == null) continue;
                
                var command = new UpgradeCommand(Faction.Handle, unit.Handle, upgradedUnitType.Handle);
                IssueCommand(command);
                
                budget -= upgradeCost;
                break;
            }
        }
        #endregion

        #region Helpers
        private ResourceAmount GetCost(Unit unitType)
        {
            return ResourceAmount.FromUnitCost(unitType, Faction);
        }

        private Unit GetNearbyUnit(Func<Unit, bool> predicate, Vector2 location)
        {
            return Faction.Units
                .Where(unit => !assignedUnits.Contains(unit) && predicate(unit))
                .WithMinOrDefault(unit => (unit.Center - location).LengthFast + GetOccupationImportance(unit));
        }

        private Unit GetMostIdleUnit(Func<Unit, bool> predicate)
        {
            return Faction.Units
                .Where(unit => !assignedUnits.Contains(unit) && predicate(unit))
                .WithMinOrDefault(unit => GetOccupationImportance(unit));
        }

        private float GetOccupationImportance(Entity entity)
        {
            Task task = entity.Components.Get<TaskQueue>().FirstOrDefault();
            if (task == null || task is MoveTask) return 0;
            if (task is HarvestTask) return 4;
            if (task is BuildTask) return 30;
            return 2;
        }

        private bool TryBuildNear(Unit builder, Unit buildingType, Vector2 location)
        {
            Point buildingLocation = new Point(
                            (int)location.X + Random.Next(-8, 9),
                            (int)location.Y + Random.Next(-8, 9));
            buildingLocation = new Region(
                World.Width - foodSupplyUnitType.Size.Width,
                World.Height - foodSupplyUnitType.Size.Height)
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

            Unit unit = (Unit)entity;
#warning Unit type comparision
            createdUnitTypes.Add(unit.Type);
            
            if (unit.IsBuilding)
            {
                buildings.Add(unit);
#warning Unit type comparision
                if (unit.Type == alageneExtractorUnitType)
                {
                    Entity node = World.Entities.Intersecting(unit.Center)
                        .First(e => e.Components.Has<Harvestable>());

                    var nodeData = resourceNodesData[node];
                    nodeData.Extractor = unit;
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

            Unit unit = (Unit)entity;
            if (unit.IsBuilding) buildings.Remove(unit);
        }
        #endregion
        #endregion
    }
}
