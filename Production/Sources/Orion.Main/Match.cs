using System;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using System.Linq;
using System.Collections.Generic;

namespace Orion.Main
{
    /// <summary>
    /// Represents a match that is played between a number of commanders in a world.
    /// </summary>
    public sealed class Match
    {
        #region Fields
        private const int campSize = 15;
        private const int initialMinimumDistanceBetweenCamps = 175;

        private readonly Random random;
        private readonly UserInputCommander userCommander;
        private readonly World world;
        private readonly CommandPipeline pipeline;
        private int lastFrameNumber = 0;
        #endregion

        #region Constructors
        internal Match(Random random, World world, UserInputCommander userCommander, CommandPipeline pipeline)
        {
            Argument.EnsureNotNull(random, "random");
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(userCommander, "userCommander");
            Argument.EnsureNotNull(pipeline, "pipeline");

            this.random = random;
            this.userCommander = userCommander;
            this.world = world;
            this.pipeline = pipeline;

            CreateFactionCamps();
            CreateResourceNodes();

            // Update the world once to force committing the entity collection operations.
            world.Update(0);
        }
        #endregion

        #region Properties
        public UserInputCommander UserCommander
        {
            get { return userCommander; }
        } 

        public World World
        {
            get { return world; }
        }

        /// <summary>
        /// Gets the number of the last frame that was ran.
        /// </summary>
        public int LastFrameNumber
        {
            get { return lastFrameNumber; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="Match"/> for the duration of a frame.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last frame, in seconds.</param>
        public void Update(float timeDeltaInSeconds)
        {
            int frameNumber = lastFrameNumber + 1;

            pipeline.Update(frameNumber, timeDeltaInSeconds);
            world.Update(timeDeltaInSeconds);

            lastFrameNumber = frameNumber;
        }

        #region Private Camp Creation
        private void CreateFactionCamps()
        {
            List<Vector2> campCenters = new List<Vector2>();
            int minimumDistanceBetweenCamps = initialMinimumDistanceBetweenCamps;

            // Find a Spot To place a base
            foreach (Faction faction in world.Factions)
            {
                Point16 campMinPosition = FindCampPosition(campCenters, ref minimumDistanceBetweenCamps);

                Vector2 campCenter = new Vector2(campMinPosition.X + campSize * 0.5f, campMinPosition.Y + campSize * 0.5f);
                campCenters.Add(campMinPosition);

                CreateCampUnits(faction, campCenter);
                CreateCampResourceNodes(campCenter);
            }
        }

        private Point16 FindCampPosition(List<Vector2> campCenters, ref int minimumDistanceBetweenCamps)
        {
            Point16 campMinPosition = new Point16(0, 0);
            int attemptCount = 0;

            while (true) // This while(true) is an euphemism for a disguised goto.
            {
                ++attemptCount;
                if (attemptCount == 750)
                {
                    minimumDistanceBetweenCamps = (int)(minimumDistanceBetweenCamps * 0.80f);
                    attemptCount = 0;
                }

                campMinPosition = new Point16((short)random.Next(world.Width), (short)random.Next(world.Height));

                if (!IsCampAreaWalkable(campMinPosition)) continue;

                // The command center is created at the center of the camp.
                Vector2 campCenter = new Vector2(campMinPosition.X + campSize * 0.5f, campMinPosition.Y + campSize * 0.5f);
                bool isNearbyAnotherCamp = false;
                foreach (Vector2 otherCampCenter in campCenters)
                {
                    if ((otherCampCenter - campCenter).Length < minimumDistanceBetweenCamps)
                    {
                        isNearbyAnotherCamp = true;
                        break;
                    }
                }

                if (isNearbyAnotherCamp) continue;

                break;
            }

            return campMinPosition;
        }

        private bool IsCampAreaWalkable(Point16 position)
        {
            for (int row = 0; row < campSize; row++)
            {
                for (int column = 0; column < campSize; column++)
                {
                    Point16 testedPoint = new Point16((short)(position.X + column), (short)(position.Y + row));
                    if (!world.IsWithinBounds(testedPoint) || !world.Terrain.IsWalkable(testedPoint))
                        return false;
                }
            }

            return true;
        }

        private void CreateResourceNodes()
        {
            for (int i = 0; i < 25; i++)
            {
                Vector2 position;
                do
                {
                    position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                } while (!world.Terrain.IsWalkable((int)position.X, (int)position.Y));
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Aladdium : ResourceType.Alagene;
                ResourceNode node = world.Entities.CreateResourceNode(resourceType, 5000, position);
            }
        }

        private void CreateCampUnits(Faction faction, Vector2 campCenter)
        {
            faction.CreateUnit(world.UnitTypes.FromName("Factory"), campCenter);
            for (int i = 1; i <= 2; i++)
            {
                faction.CreateUnit(world.UnitTypes.FromName("Builder"), campCenter + new Vector2(i * campSize / 8f, 0));
                faction.CreateUnit(world.UnitTypes.FromName("Harvester"), campCenter + new Vector2(0, i * campSize / 8f));
            }
        }

        private void CreateCampResourceNodes(Vector2 campCenter)
        {
            world.Entities.CreateResourceNode(ResourceType.Aladdium, 5000,
                campCenter + new Vector2(campSize / -4f, campSize / -4f));

            world.Entities.CreateResourceNode(ResourceType.Alagene, 5000,
                campCenter + new Vector2(campSize / -4f, campSize / 4f));
        }
        #endregion
        #endregion
    }
}
