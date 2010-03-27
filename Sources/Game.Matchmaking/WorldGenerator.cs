using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using OpenTK.Math;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Generates random contents in a world.
    /// </summary>
    public static class WorldGenerator
    {
        #region Fields
        private const double resourcesDensity = 0.00518798828125;
        private const int campSize = 15;
        private const float initialMinimumDistanceBetweenCamps = 50;
        #endregion

        #region Methods
        public static void Generate(World world, Random random, bool nomad)
        {
            foreach (Faction faction in world.Factions)
                GenerateFactionCamp(world, faction, random, nomad);
            
            GenerateResourceNodes(world, random);
        }

        private static void GenerateResourceNodes(World world, Random random)
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

        private static void GenerateResourceNodes(World world, int aladdiumNodeCount, int alageneNodeCount, Random random)
        {
            for (int i = 0; i < aladdiumNodeCount + alageneNodeCount; i++)
            {
                Point location = GetRandomFreeLocation(world, ResourceNode.DefaultSize, random);
                ResourceType resourceType = i < aladdiumNodeCount ? ResourceType.Aladdium : ResourceType.Alagene;
                ResourceNode node = world.Entities.CreateResourceNode(resourceType, location);
            }
        }

        private static void GenerateFactionCamp(World world, Faction faction, Random random, bool placePyramid)
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

            CreateCamp(world, faction, campCenter, placePyramid);
        }

        private static void CreateCamp(World world, Faction faction, Vector2 campCenter, bool placePyramid)
        {
            Region buildingRegion;
            UnitType pyramid = world.UnitTypes.FromName("Pyramide");
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

            UnitType unitType = world.UnitTypes.FromName("Schtroumpf");
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX, buildingRegion.MinY));
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX, buildingRegion.MinY + 1));
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX + 1, buildingRegion.MinY));
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX + 1, buildingRegion.MinY + 1));

            world.Entities.CreateResourceNode(ResourceType.Aladdium,
                (Point)(campCenter + new Vector2(campSize / -4f, campSize / -4f)));

            world.Entities.CreateResourceNode(ResourceType.Alagene,
                (Point)(campCenter + new Vector2(campSize / -4f, campSize / 4f)));
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
        #endregion
    }
}
