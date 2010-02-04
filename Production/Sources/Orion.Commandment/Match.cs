using System;
using OpenTK.Math;
using Orion.Commandment;
using Orion.Commandment.Commands.Pipeline;
using Orion.GameLogic;
using System.Linq;
using System.Collections.Generic;

namespace Orion.Commandment
{
    /// <summary>
    /// Represents a match that is played between a number of commanders in a world.
    /// </summary>
    public sealed class Match
    {
        #region Fields
        private const double resourcesRatio = 0.00518798828125;
        private const int campSize = 15;
        private const int initialMinimumDistanceBetweenCamps = 175;

        private readonly Random random;
        private readonly World world;
        private SimulationStep lastSimulationStep;
        #endregion

        #region Constructors
        public Match(Random random, World world)
        {
            Argument.EnsureNotNull(random, "random");
            Argument.EnsureNotNull(world, "world");

            this.random = random;
            this.world = world;

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
        /// Raised when allied factions defeat all their ennemies.
        /// </summary>
        public event GenericEventHandler<Match, IEnumerable<Faction>> WorldConquered;

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

        private void RaiseWorldConquered(IEnumerable<Faction> faction)
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
        public World World
        {
            get { return world; }
        }

        public bool IsPausable { get; set; }

        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the number of the last simulation step that was run.
        /// </summary>
        public int LastSimulationStepNumber
        {
            get { return lastSimulationStep.Number; }
        }

        /// <summary>
        /// Gets the information associated with the last step of the game simulation.
        /// </summary>
        public SimulationStep LastSimulationStep
        {
            get { return lastSimulationStep; }
        }
        #endregion

        #region Methods
        public void Start()
        {
            IsRunning = true;

            CreateFactionCamps();
            CreateResourceNodes();
        }

        /// <summary>
        /// Updates this <see cref="Match"/> for the duration of a frame.
        /// </summary>
        /// <param name="timeDelta">The time elapsed, in seconds, since the last frame.</param>
        public void Update(float timeDeltaInSeconds)
        {
            if (IsRunning)
            {
                lastSimulationStep = new SimulationStep(
                    lastSimulationStep.Number + 1,
                    lastSimulationStep.TimeInSeconds + timeDeltaInSeconds,
                    timeDeltaInSeconds);

                world.Update(lastSimulationStep);
            }

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
            List<Faction> aliveFactions = world.Factions.Where(faction => faction.Status == FactionStatus.Undefeated).ToList();
            for (int i = 0; i < aliveFactions.Count; i++)
            {
                Faction referenceFaction = aliveFactions[i];
                for (int j = i + 1; j < aliveFactions.Count; j++)
                {
                    Faction checkedFaction = aliveFactions[j];
                    DiplomaticStance referenceDiplomaticStance = referenceFaction.GetDiplomaticStance(checkedFaction);
                    DiplomaticStance checkedDiplomaticStance = checkedFaction.GetDiplomaticStance(referenceFaction);
                    if (referenceDiplomaticStance != DiplomaticStance.Ally || checkedDiplomaticStance != DiplomaticStance.Ally)
                        return;
                }
            }
            if (aliveFactions.Count() == 1) RaiseWorldConquered(aliveFactions);
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
            Point campLocation = new Point(0, 0);
            int attemptCount = 0;

            while (true) // This while(true) is an euphemism for a disguised goto.
            {
                ++attemptCount;
                if (attemptCount == 750)
                {
                    minimumDistanceBetweenCamps = (int)(minimumDistanceBetweenCamps * 0.80f);
                    attemptCount = 0;
                }

                campLocation = new Point(random.Next(world.Size.Width), random.Next(world.Size.Height));

                if (!IsCampAreaWalkable(campLocation)) continue;

                // The command center is created at the center of the camp.
                Vector2 campCenter = new Vector2(campLocation.X + campSize * 0.5f, campLocation.Y + campSize * 0.5f);
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

            return campLocation;
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
            int resourceNodeCount = (int)(world.Terrain.Width * world.Terrain.Height * resourcesRatio);
            for (int i = 0; i < resourceNodeCount; i++)
            {
                Point location = GetFreeLocation(ResourceNode.DefaultSize);
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Aladdium : ResourceType.Alagene;
                ResourceNode node = world.Entities.CreateResourceNode(resourceType, location);
            }
        }

        private Point GetFreeLocation(Size size)
        {
            while (true)
            {
                Point location = new Point(
                    random.Next(world.Size.Width - size.Width),
                    random.Next(world.Size.Height - size.Height));

                Region region = new Region(location, size);

                bool isWalkable = world.Terrain.IsWalkable(region);
                if (!isWalkable) continue;

                bool isFreeOfEntities = world.Entities
                    .Intersecting(region.ToRectangle())
                    .None(entity => Region.Intersects(entity.GridRegion, region));
                if (!isFreeOfEntities) continue;

                return location;
            }
        }

        private void CreateCampUnits(Faction faction, Vector2 campCenter)
        {
            Unit building = faction.CreateUnit(world.UnitTypes.FromName("Pyramide"), (Point)campCenter);
            building.CompleteConstruction();
            Region buildingRegion = building.GridRegion;

            UnitType unitType = world.UnitTypes.FromName("Schtroumpf");
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX, buildingRegion.MinY));
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX, buildingRegion.MinY + 1));
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX + 1, buildingRegion.MinY));
            faction.CreateUnit(unitType, new Point(buildingRegion.ExclusiveMaxX + 1, buildingRegion.MinY + 1));
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
