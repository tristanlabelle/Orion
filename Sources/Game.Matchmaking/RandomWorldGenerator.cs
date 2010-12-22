using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Engine.Collections;
using Orion.Engine;
using OpenTK;

namespace Orion.Game.Matchmaking
{
    public class RandomWorldGenerator : WorldGenerator
    {
        #region Fields
        private readonly double resourcesDensity;
        private readonly int campSize;
        private readonly float initialMinimumDistanceBetweenCamps;
        private readonly Random random;
        private readonly bool createPyramids;
        #endregion

        #region Constructors
        public RandomWorldGenerator(Random random, bool createPyramids)
        {
            resourcesDensity = 0.00518798828125;
            campSize = 15;
            initialMinimumDistanceBetweenCamps = 50;
            this.random = random;
            this.createPyramids = createPyramids;
        }
        #endregion

        #region Methods
        public override void Generate(World world, UnitTypeRegistry unitTypes)
        {
            foreach (Faction faction in world.Factions)
                GenerateFactionCamp(world, unitTypes, faction, random, createPyramids);

            GenerateResourceNodes(world, random);
        }

        private void GenerateResourceNodes(World world, Random random)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(random, "random");

            GenerateResourceNodes(world, resourcesDensity, random);
        }

        private static void GenerateResourceNodes(World world, double density, Random random)
        {
            int resourceNodeCount = (int)(world.Size.Area * density) / 2;
            GenerateResourceNodes(world, resourceNodeCount, resourceNodeCount, random);
        }

        private static void GenerateResourceNodes(World world,
            int aladdiumNodeCount, int alageneNodeCount, Random random)
        {
            for (int i = 0; i < aladdiumNodeCount + alageneNodeCount; i++)
            {
                Point location = GetRandomFreeLocation(world, ResourceNode.DefaultSize, random);
                ResourceType resourceType = i < aladdiumNodeCount ? ResourceType.Aladdium : ResourceType.Alagene;
                ResourceNode node = world.Entities.CreateResourceNode(resourceType, location);
            }
        }

        private static Point GetRandomFreeLocation(World world, Size regionSize, Random random)
        {
            while (true)
            {
                Point location = new Point(
                    random.Next(world.Size.Width - regionSize.Width),
                    random.Next(world.Size.Height - regionSize.Height));

                Region region = new Region(location, regionSize);

                bool isWalkable = world.Terrain.IsWalkable(region);
                if (!isWalkable) continue;

                bool isFreeOfEntities = world.Entities
                    .Intersecting(region.ToRectangle())
                    .None(entity => Region.Intersects(entity.GridRegion, region));
                if (!isFreeOfEntities) continue;

                return location;
            }
        }

        private void GenerateFactionCamp(World world, UnitTypeRegistry unitTypes,
            Faction faction, Random random, bool placePyramid)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(random, "random");

            world.Entities.CommitDeferredChanges();

            List<Vector2> buildingPositions = world.Entities
                .OfType<Unit>()
                .Where(unit => unit.IsBuilding)
                .Select(unit => unit.Center)
                .ToList();

            Vector2 campCenter = Vector2.Zero;
            int attemptCount = 0;

            float minimumDistanceBetweenCamps = initialMinimumDistanceBetweenCamps;

            while (true)
            {
                ++attemptCount;
                if (attemptCount == 200)
                {
                    minimumDistanceBetweenCamps *= 0.80f;
                    attemptCount = 0;
                }

                Point campLocation = new Point(random.Next(world.Size.Width - campSize), random.Next(world.Size.Height - campSize));
                Region campRegion = new Region(campLocation.X, campLocation.Y, campSize, campSize);

                if (!world.IsFree(campRegion, CollisionLayer.Ground)) continue;

                campCenter = new Vector2(campLocation.X + campSize * 0.5f, campLocation.Y + campSize * 0.5f);

                bool isNearbyAnotherCamp = buildingPositions
                    .Any(position => (position - campCenter).LengthSquared < minimumDistanceBetweenCamps * minimumDistanceBetweenCamps);
                if (isNearbyAnotherCamp) continue;

                break;
            }

            CreateCamp(world, unitTypes, faction, campCenter, placePyramid);
        }

        private void CreateCamp(World world, UnitTypeRegistry unitTypes,
            Faction faction, Vector2 campCenter, bool placePyramid)
        {
            Region buildingRegion;
            UnitType pyramid = unitTypes.FromName("Pyramide");
            if (placePyramid)
            {
                Unit building = faction.CreateUnit(pyramid, (Point)campCenter);
                building.CompleteConstruction();
                buildingRegion = building.GridRegion;
            }
            else
            {
                buildingRegion = new Region((Point)campCenter, pyramid.Size);
            }

            UnitType unitType = unitTypes.FromName("Schtroumpf");
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX, buildingRegion.MinY));
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX, buildingRegion.MinY + 1));
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX + 1, buildingRegion.MinY));
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX + 1, buildingRegion.MinY + 1));

            world.Entities.CreateResourceNode(ResourceType.Aladdium,
                (Point)(campCenter + new Vector2(campSize / -4f, campSize / -4f)));

            world.Entities.CreateResourceNode(ResourceType.Alagene,
                (Point)(campCenter + new Vector2(campSize / -4f, campSize / 4f)));
        }
        #endregion
    }
}
