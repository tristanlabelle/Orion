using System;
using OpenTK.Math;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using System.Linq;
using System.Collections.Generic;
using Skills = Orion.GameLogic.Skills;

namespace Orion.Commandment
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
        private readonly World world;
        private readonly UserInputCommander userCommander;
        private int lastFrameNumber = 0;
        #endregion

        #region Constructors
        public Match(Random random, World world, UserInputCommander commander)
        {
            Argument.EnsureNotNull(random, "random");
            Argument.EnsureNotNull(world, "world");

            this.random = random;
            this.world = world;
            userCommander = commander;
            IsRunning = true;

            CreateFactionCamps();
            CreateResourceNodes();

            // Update the world once to force committing the entity collection operations.
            world.Update(0);

            world.FactionDefeated += OnFactionDefeated;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Match"/> gets updated for a frame.
        /// </summary>
        public event GenericEventHandler<Match, UpdateEventArgs> Updated;

        /// <summary>
        /// Raised when a message was received from a <see cref="Faction"/>.
        /// </summary>
        public event GenericEventHandler<Match, FactionMessage> FactionMessageReceived;

        /// <summary>
        /// Raised when all <see cref="Faction"/>s but one are defeated.
        /// </summary>
        public event GenericEventHandler<Match, Faction> WorldConquered;

        public event GenericEventHandler<Match> Quitting;

        private void RaiseUpdated(float timeDeltaInSeconds)
        {
            var handler = Updated;
            if (handler != null) handler(this, new UpdateEventArgs(timeDeltaInSeconds));
        }

        private void RaiseFactionMessageReceived(FactionMessage message)
        {
            var handler = FactionMessageReceived;
            if (handler != null) handler(this, message);
        }

        private void RaiseWorldConquered(Faction faction)
        {
            var handler = WorldConquered;
            if (handler != null) handler(this, faction);
        }

        private void RaiseQuitting()
        {
            var handler = Quitting;
            if (handler != null) handler(this);
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

        public bool IsPausable { get; set; }

        public bool IsRunning { get; private set; }

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
        /// <param name="timeDelta">The time elapsed, in seconds, since the last frame.</param>
        public void Update(float timeDeltaInSeconds)
        {
            if (IsRunning)
            {
                world.Update(timeDeltaInSeconds);
            }

            lastFrameNumber++;
            RaiseUpdated(timeDeltaInSeconds);
        }

        #region Faction Stuff
        private void OnFactionDefeated(World sender, Faction args)
        {
            CheckForWorldConquered();
        }

        /// <summary>
        /// Notifies this <see cref="Match"/> that a message from a <see cref="Faction"/> has been received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        public void PostFactionMessage(FactionMessage message)
        {
            Argument.EnsureNotNull(message, "message");
            RaiseFactionMessageReceived(message);
        }

        private void CheckForWorldConquered()
        {
            var aliveFactions = world.Factions.Where(faction => faction.Status == FactionStatus.Undefeated);
            if (aliveFactions.Count() == 1) RaiseWorldConquered(aliveFactions.First());
        }
        #endregion

        #region Pause/Resume/Quit
        public void Pause()
        {
            IsRunning = false;
        }

        public void Resume()
        {
            IsRunning = true;
        }

        public void Quit()
        {
            RaiseQuitting();
        }
        #endregion

        #region Private Camp Creation
        private void CreateFactionCamps()
        {
            List<Vector2> campCenters = new List<Vector2>();
            int minimumDistanceBetweenCamps = initialMinimumDistanceBetweenCamps;

            // Find a Spot To place a base
            foreach (Faction faction in world.Factions)
            {
                Point campMinPosition = FindCampPosition(campCenters, ref minimumDistanceBetweenCamps);

                Vector2 campCenter = new Vector2(campMinPosition.X + campSize * 0.5f, campMinPosition.Y + campSize * 0.5f);
                campCenters.Add(campMinPosition);

                CreateCampUnits(faction, campCenter);
                CreateCampResourceNodes(campCenter);
            }
        }

        private Point FindCampPosition(List<Vector2> campCenters, ref int minimumDistanceBetweenCamps)
        {
            Point campMinPosition = new Point(0, 0);
            int attemptCount = 0;

            while (true) // This while(true) is an euphemism for a disguised goto.
            {
                ++attemptCount;
                if (attemptCount == 750)
                {
                    minimumDistanceBetweenCamps = (int)(minimumDistanceBetweenCamps * 0.80f);
                    attemptCount = 0;
                }

                campMinPosition = new Point(random.Next(world.Size.Width), random.Next(world.Size.Height));

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

        private bool IsCampAreaWalkable(Point position)
        {
            for (int row = 0; row < campSize; row++)
            {
                for (int column = 0; column < campSize; column++)
                {
                    Point testedPoint = new Point(position.X + column, position.Y + row);
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
                Point location;
                do
                {
                    location = new Point(
                        random.Next(world.Size.Width - ResourceNode.DefaultSize.Width),
                        random.Next(world.Size.Height - ResourceNode.DefaultSize.Height));
                } while (!world.Terrain.IsWalkable(location));
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Aladdium : ResourceType.Alagene;
                ResourceNode node = world.Entities.CreateResourceNode(resourceType, location);
            }
        }

        private void CreateCampUnits(Faction faction, Vector2 campCenter)
        {
            Unit pyramide = faction.CreateUnit(world.UnitTypes.FromName("Pyramide"), (Point)campCenter);
            pyramide.CompleteConstruction();
            for (int i = 1; i <= 2; i++)
            {
                faction.CreateUnit(world.UnitTypes.FromName("Schtroumpf"),
                    (Point)(campCenter + new Vector2(i * campSize / 8f, 0)));
            }
            for (int i = 1; i <= 2; i++)
            {
                faction.CreateUnit(world.UnitTypes.FromName("Schtroumpf"),
                    (Point)(campCenter + new Vector2(i * campSize / 8f, 1)));
            }
        }

        private void CreateCampResourceNodes(Vector2 campCenter)
        {
            world.Entities.CreateResourceNode(ResourceType.Aladdium,
                (Point)(campCenter + new Vector2(campSize / -4f, campSize / -4f)));

            world.Entities.CreateResourceNode(ResourceType.Alagene,
                (Point)(campCenter + new Vector2(campSize / -4f, campSize / 4f)));
        }
        #endregion
        #endregion
    }
}
