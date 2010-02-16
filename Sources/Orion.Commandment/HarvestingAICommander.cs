using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.GameLogic.Tasks;
using OpenTK.Math;
using System.Diagnostics;

namespace Orion.Matchmaking
{
    public sealed class HarvestingAICommander : SlaveCommander
    {
        #region Fields
        private const int maxHarvestersPerNode = 6;
        private const float maxResourceNodeToDepotDistance = 15;

        private readonly Random random;
        private readonly UnitType harvesterType;
        private readonly UnitType alageneExtractorType;
        private readonly UnitType scoutType;
        private readonly UnitType resourceStorageType;
        private readonly UnitType foodStorageType;
        private float accumulatedTimeDelta;
        private bool hasTriedResolvingResourcesNeed;
        #endregion

        #region Constructors
        public HarvestingAICommander(Faction faction, Random random)
            : base(faction)
        {
            Argument.EnsureNotNull(random, "random");

            this.random = random;
            this.harvesterType = World.UnitTypes
                .First(unitType => unitType.HasSkill<HarvestSkill>() && unitType.HasSkill<MoveSkill>());
            this.alageneExtractorType = World.UnitTypes
                .First(unitType => unitType.HasSkill<ExtractAlageneSkill>());
            this.scoutType = World.UnitTypes
                .Where(unitType => unitType.HasSkill<MoveSkill>())
                .WithMax(unitType => Faction.GetStat(unitType, UnitStat.MovementSpeed));
            this.resourceStorageType = World.UnitTypes
                .First(unitType => unitType.HasSkill<StoreResourcesSkill>());
            this.foodStorageType = World.UnitTypes
                .First(unitType => unitType.HasSkill<StoreFoodSkill>());
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            accumulatedTimeDelta += timeDelta;
            if (accumulatedTimeDelta < 1) return;

            hasTriedResolvingResourcesNeed = false;
            ResolveVictoryNeed();

            accumulatedTimeDelta = 0;
        }

        public void ResolveVictoryNeed()
        {
            Unit target = World.Entities
                .OfType<Unit>()
                .Where(unit => Faction.CanSee(unit))
                .FirstOrDefault(unit => Faction.GetDiplomaticStance(unit.Faction) == DiplomaticStance.Enemy);

            if (target != null) ResolveAttackNeed(target);
            else ResolveDefendNeed();
        }

        private void ResolveAttackNeed(Unit target)
        {
            var attackers = Faction.Units
                .Where(unit => unit.IsIdle && unit.HasSkill<AttackSkill>() && unit.HasSkill<MoveSkill>());

            if (attackers.Any()) LaunchAttack(attackers, target);
            else ResolveAttackersNeed();
        }

        private void ResolveAttackersNeed()
        {
            UnitType traineeType = World.UnitTypes
                .First(unitType => unitType.HasSkill<MoveSkill>()
                    && unitType.HasSkill<AttackSkill>()
                    && !unitType.HasSkill<HarvestSkill>());

            ResolveUnitNeed(traineeType);
        }

        private void ResolveUnitNeed(UnitType unitType)
        {
            if (Faction.RemainingFoodAmount < unitType.FoodCost)
            {
                ResolveFoodNeed();
                return;
            }

            int aladdiumCost = Faction.GetStat(unitType, UnitStat.AladdiumCost);
            int alageneCost = Faction.GetStat(unitType, UnitStat.AlageneCost);
            if (!TryResolveResourceNeed(aladdiumCost, alageneCost))
                return;

            var trainers = Faction.Units
                .Where(unit => !unit.TaskQueue.IsFull && unit.HasSkill<TrainSkill>()
                    && unit.GetSkill<TrainSkill>().Supports(unitType));

            if (!trainers.Any())
            {
                UnitType trainerType = World.UnitTypes
                    .First(t => t.HasSkill<TrainSkill>()
                        && t.GetSkill<TrainSkill>().Supports(unitType));

                ResolveBuildingNeed(trainerType);
                return;
            }

            LaunchTrain(trainers, unitType);
        }

        private void ResolveFoodNeed()
        {
            ResolveBuildingNeed(foodStorageType);
        }

        private bool TryResolveResourceNeed(int aladdiumCost, int alageneCost)
        {
            bool needsAladdium = Faction.AladdiumAmount < aladdiumCost;
            bool needsAlagene = Faction.AladdiumAmount < alageneCost;

            if (!needsAladdium && !needsAlagene) return true;

            // Prevent stack overflow
            if (hasTriedResolvingResourcesNeed) return false;
            hasTriedResolvingResourcesNeed = true;

            if (needsAladdium)
            {
                ResolveResourceNeed(ResourceType.Aladdium);
                return false;
            }

            if (needsAlagene)
            {
                ResolveResourceNeed(ResourceType.Alagene);
                return false;
            }

            return false;
        }

        private void ResolveResourceNeed(ResourceType type)
        {
            var idleHarvesters = Faction.Units.Where(unit => unit.IsIdle && unit.Type == harvesterType);

            if (!idleHarvesters.Any())
            {
                ResolveUnitNeed(harvesterType);
                return;
            }

            var visibleNodes = World.Entities
                .OfType<ResourceNode>()
                .Where(n => n.Type == type && Faction.HasPartiallySeen(n.GridRegion));

            var pair = visibleNodes.Select(n =>
                new KeyValuePair<ResourceNode, int>(n, Faction.Units
                    .Select(unit => unit.TaskQueue.Current as HarvestTask)
                    .Count(task => task != null && task.ResourceNode == n)))
                .WithMinOrDefault(p => p.Value);

            ResourceNode node = pair.Key;
            int harvestersOnNodeCount = pair.Value;
            if (node == null || harvestersOnNodeCount >= maxHarvestersPerNode)
            {
                ResolveExploreNeed();
                return;
            }

            Unit nearestDepot = Faction.Units
                .Where(unit => unit.Type == resourceStorageType)
                .WithMinOrDefault(depot => (depot.Center - node.Center).LengthSquared);

            if (nearestDepot == null || (nearestDepot.Center - node.Center).LengthSquared
                > maxResourceNodeToDepotDistance * maxResourceNodeToDepotDistance)
            {
                ResolveNearbyBuildingNeed(resourceStorageType, node.GridRegion);
                return;
            }

            if (type == ResourceType.Alagene)
            {
                if (node.Extractor == null)
                {
                    ResolveBuildingNeed(alageneExtractorType, pair.Key.GridRegion.Min);
                    return;
                }

                if (node.Extractor.Faction != Faction)
                {
                    ResolveAttackNeed(node.Extractor);
                    return;
                }
            }

            LaunchHarvest(idleHarvesters.Take(5), node);
        }

        private void ResolveExploreNeed()
        {
            var explorer = Faction.Units
                .FirstOrDefault(unit => unit.IsIdle && unit.HasSkill<MoveSkill>());

            if (explorer == null)
            {
                ResolveUnitNeed(scoutType);
                return;
            }

            for (int i = 0; i < 100; ++i)
            {
                float angle = i * (float)Math.PI * 0.1f;
                float distance = i * 3;
                
                float x = (float)Math.Cos(angle) * distance;
                float y = (float)Math.Sin(angle) * distance;
                Vector2 target = explorer.Center + new Vector2(x, y);
                
                Point targetPoint = (Point)target;

                if (World.IsWithinBounds(targetPoint)
                    && Faction.GetTileVisibility(targetPoint) == TileVisibility.Undiscovered)
                {
                    LaunchMove(new[] { explorer }, targetPoint);
                    return;
                }
            }

            Debug.Fail("Nothing left to explore?");
        }

        private void ResolveNearbyBuildingNeed(UnitType buildingType, Region region)
        {
            Unit builder = GetBuilderOrResolveNeed(buildingType, region.Min);
            if (builder == null) return;

            BuildNearRegion(builder, buildingType, region);
        }

        private void ResolveBuildingNeed(UnitType buildingType)
        {
            Unit builder = GetBuilderOrResolveNeed(buildingType,null);
            if (builder == null) return;

            Region region = Region.Grow(builder.GridRegion, 6);
            BuildNearRegion(builder, buildingType, region);
        }

        private Unit GetBuilderOrResolveNeed(UnitType buildingType, Vector2? locationHint)
        {
            int aladdiumCost = Faction.GetStat(buildingType, UnitStat.AladdiumCost);
            int alageneCost = Faction.GetStat(buildingType, UnitStat.AlageneCost);
            if (!TryResolveResourceNeed(aladdiumCost, alageneCost))
                return null;

            UnitType builderType = World.UnitTypes
                .First(unitType => unitType.HasSkill<BuildSkill>()
                    && unitType.GetSkill<BuildSkill>().Supports(buildingType));

            var builders = Faction.Units.Where(unit => unit.Type == builderType);

            Unit builder = locationHint.HasValue
                ? builders.WithMinOrDefault(unit => (unit.Center - locationHint.Value).LengthSquared)
                : builders.FirstOrDefault();

            if (builder == null || !builder.IsIdle)
            {
                if (builder != null && buildingType == foodStorageType)
                {
                    // If we want to build a food storage, we have no choice but to
                    // take a builder and cancel its task.
                    LaunchCancel(new[] { builder });
                    return null;
                }

                ResolveUnitNeed(builderType);
                return null;
            }

            return builder;
        }

        private void ResolveBuildingNeed(UnitType buildingType, Point point)
        {
            Unit builder = GetBuilderOrResolveNeed(buildingType, point);
            if (builder == null) return;

            LaunchBuild(new[] { builder }, buildingType, point);
        }

        private void BuildNearRegion(Unit builder, UnitType buildingType, Region region)
        {
            while (true)
            {
                foreach (Point buildingMin in region.GetAdjacentPoints())
                {
                    Region buildingRegion = new Region(buildingMin, buildingType.Size);
                    if (!((Region)World.Size).Contains(buildingRegion)) continue;
                    if (!World.IsFree(buildingRegion, CollisionLayer.Ground)) continue;
                    LaunchBuild(new[] { builder }, buildingType, buildingMin);
                    return;
                }
                region = Region.Grow(region, 1);
            }
        }


        private void ResolveDefendNeed()
        {
            ResolveAttackersNeed();
        }
        #endregion
    }
}
