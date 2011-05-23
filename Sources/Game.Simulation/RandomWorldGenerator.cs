using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// A world generator which generates random topology using perlin noise.
    /// </summary>
    public sealed class RandomWorldGenerator : WorldGenerator
    {
        #region Fields
        private readonly double resourcesDensity;
        private readonly int campSize;
        private readonly float initialMinimumDistanceBetweenCamps;
        private readonly Random random;
        private readonly bool createPyramids;
        private readonly Size terrainSize;
        #endregion

        #region Constructors
        public RandomWorldGenerator(Random random, Size terrainSize, bool createPyramids)
        {
            resourcesDensity = 0.00518798828125;
            campSize = 15;
            initialMinimumDistanceBetweenCamps = 50;
            this.random = random;
            this.terrainSize = terrainSize;
            this.createPyramids = createPyramids;
        }
        #endregion

        #region Methods
        #region Overrides
        public override void PrepareWorld(World world, PrototypeRegistry prototypes)
        {
            foreach (Faction faction in world.Factions)
                GenerateFactionCamp(world, prototypes, faction, createPyramids);

            GenerateResourceNodes(world, prototypes);
        }

        public override Terrain GenerateTerrain()
        {
            PerlinNoise noise = new PerlinNoise(random);

            BitArray2D tiles = new BitArray2D(terrainSize);
            double[] rawTerrain = new double[terrainSize.Area];
            for (int y = 0; y < terrainSize.Height; y++)
            {
                for (int x = 0; x < terrainSize.Width; x++)
                {
                    rawTerrain[y * terrainSize.Width + x] = noise[x, y];
                }
            }

            double max = rawTerrain.Max();
            int k = 0;
            foreach (double noiseValue in rawTerrain.Select(d => d / max))
            {
                tiles[k % terrainSize.Width, k / terrainSize.Width] = noiseValue >= 0.5;
                k++;
            }

            return new Terrain(tiles);
        }
        #endregion

        #region Private
        private void GenerateResourceNodes(World world, PrototypeRegistry prototypes)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(prototypes, "prototypes");

            int count = (int)(world.Size.Area * resourcesDensity) / 2;
            for (int i = 0; i < count * 2; i++)
            {
                ResourceType type = i % 2 == 0 ? ResourceType.Aladdium : ResourceType.Alagene;
                Entity prototype = prototypes.FromResourceType(type);
                Point location = GetRandomFreeLocation(world, prototype.Spatial.Size);
                CreateResourceNode(world, prototypes, type, location);
            }
        }

        private Point GetRandomFreeLocation(World world, Size regionSize)
        {
            while (true)
            {
                Point location = new Point(
                    random.Next(world.Size.Width - regionSize.Width),
                    random.Next(world.Size.Height - regionSize.Height));

                Region region = new Region(location, regionSize);

                bool isWalkable = world.Terrain.IsWalkable(region);
                if (!isWalkable) continue;

                bool isFreeOfEntities = world.SpatialManager
                    .Intersecting(region.ToRectangle())
                    .None(spatial => Region.Intersects(spatial.GridRegion, region));
                if (!isFreeOfEntities) continue;

                return location;
            }
        }

        private void GenerateFactionCamp(World world, PrototypeRegistry prototypes,
            Faction faction, bool placePyramid)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(prototypes, "prototypes");
            Argument.EnsureNotNull(faction, "faction");

            world.Entities.CommitDeferredChanges();

            var buildingPositions = world.Entities
                .Where(entity => entity.Identity.IsBuilding)
                .Select(entity => entity.Center)
                .NonDeferred();

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

            CreateCamp(world, prototypes, faction, campCenter, placePyramid);
        }

        private void CreateCamp(World world, PrototypeRegistry prototypes,
            Faction faction, Vector2 campCenter, bool placePyramid)
        {
            Region buildingRegion;
            Entity commandCenterPrototype = prototypes.FromName("Pyramid");
            if (placePyramid)
            {
                Entity building = faction.CreateUnit(commandCenterPrototype, (Point)campCenter);
                buildingRegion = building.Spatial.GridRegion;
            }
            else
            {
                buildingRegion = new Region((Point)campCenter, commandCenterPrototype.Spatial.Size);
            }

            Entity workerPrototype = prototypes.FromName("Smurf");
            faction.CreateUnit(workerPrototype, new Point(buildingRegion.ExclusiveMaxX, buildingRegion.MinY));
            faction.CreateUnit(workerPrototype, new Point(buildingRegion.ExclusiveMaxX, buildingRegion.MinY + 1));
            faction.CreateUnit(workerPrototype, new Point(buildingRegion.ExclusiveMaxX + 1, buildingRegion.MinY));
            faction.CreateUnit(workerPrototype, new Point(buildingRegion.ExclusiveMaxX + 1, buildingRegion.MinY + 1));

            CreateResourceNode(world, prototypes, ResourceType.Aladdium,
                (Point)(campCenter + new Vector2(campSize / -4f, campSize / -4f)));

            CreateResourceNode(world, prototypes, ResourceType.Alagene,
                (Point)(campCenter + new Vector2(campSize / -4f, campSize / 4f)));
        }
        #endregion
        #endregion
    }
}
