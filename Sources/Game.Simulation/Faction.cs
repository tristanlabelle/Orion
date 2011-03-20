using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Pathfinding;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Technologies;
using ColorPalette = Orion.Engine.Colors;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Represents a faction, a group of allied units sharing resources and sharing a goal.
    /// </summary>
    [Serializable]
    public sealed class Faction
    {
    	#region Static
    	public static bool HaveAlliedVictory(Faction a, Faction b)
    	{
    		return a.GetDiplomaticStance(b).HasFlag(DiplomaticStance.AlliedVictory)
    			&& b.GetDiplomaticStance(a).HasFlag(DiplomaticStance.AlliedVictory);
    	}
    	#endregion
    	
    	#region Instance
        #region Fields
        private const int minimumFoodAmount = 10;

        private readonly Handle handle;
        private readonly World world;
        private readonly string name;
        private readonly ColorRgb color;
        private readonly FogOfWar localFogOfWar;

        private readonly Dictionary<Faction, DiplomaticStance> diplomaticStances = new Dictionary<Faction, DiplomaticStance>();

        private readonly HashSet<Technology> researches = new HashSet<Technology>();
        private readonly HashSet<Technology> technologies = new HashSet<Technology>();

        private readonly Action<FogOfWar, Region> fogOfWarChangedEventHandler;

        private int aladdiumAmount;
        private int alageneAmount;

        /// <summary>
        /// The amount of food provided by all food-storing units.
        /// The actual food limit is clamped within a range such as [10, 200].
        /// </summary>
        private int totalFoodAmount;
        private int usedFoodAmount;
        private FactionStatus status = FactionStatus.Undefeated;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Faction"/> from its name and <see cref="Color"/>.
        /// </summary>
        /// <param name="handle">The handle of this <see cref="Faction"/>.</param>
        /// <param name="world">The <see cref="World"/> hosting this <see cref="Faction"/>.</param>
        /// <param name="name">The name of this <see cref="Faction"/>.</param>
        /// <param name="color">The distinctive <see cref="ColorRgb"/> of this <see cref="Faction"/>'s units.</param>
        internal Faction(Handle handle, World world, string name, ColorRgb color)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNullNorBlank(name, "name");

            diplomaticStances[this] = DiplomaticStance.ForeverAllied;

            this.handle = handle;
            this.world = world;
            this.name = name;
            this.color = color;
            this.localFogOfWar = new FogOfWar(world.Size);

            this.fogOfWarChangedEventHandler = OnFogOfWarChanged;
            this.localFogOfWar.Changed += fogOfWarChangedEventHandler;
            this.world.EntityAdded += OnWorldEntityAdded;
            this.world.EntityRemoved += OnWorldEntityRemoved;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Faction"/> gets defeated.
        /// </summary>
        public event Action<Faction> Defeated;

        /// <summary>
        /// Raised when the area of the world that is visible by this faction changes.
        /// </summary>
        public event Action<Faction, Region> VisibilityChanged;

        /// <summary>
        /// Raised when a new <see cref="Technology"/> has been researched.
        /// </summary>
        public event Action<Faction, Technology> TechnologyResearched;

        /// <summary>
        /// Raised when the faction changes its diplomatic stance with regard to another faction.
        /// </summary>
        public event Action<Faction, DiplomaticStanceChange> DiplomaticStanceChanged;

        public event Action<Faction> AladdiumAmountChanged;
        public event Action<Faction> AlageneAmountChanged;
        public event Action<Faction> UsedFoodAmountChanged;
        public event Action<Faction> MaxFoodAmountChanged;

        public event Action<Faction, string> Warning;

        public void RaiseWarning(string message)
        {
            Warning.Raise(this, message);
        }
        #endregion

        #region Properties
        #region Identification
        /// <summary>
        /// Gets the handle of this <see cref="Faction"/>.
        /// </summary>
        public Handle Handle
        {
            get { return handle; }
        }

        /// <summary>
        /// Gets the name of this <see cref="Faction"/>.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the <see cref="Color"/> used to visually identify units of this <see cref="Faction"/>.
        /// </summary>
        public ColorRgb Color
        {
            get { return color; }
        }
        #endregion

        /// <summary>
        /// Gets the <see cref="World"/> which hosts this faction.
        /// </summary>
        public World World
        {
            get { return world; }
        }

        /// <summary>
        /// Gets the local fog of war of this faction, which does not take allies into account.
        /// </summary>
        public FogOfWar LocalFogOfWar
        {
            get { return localFogOfWar; }
        }

        /// <summary>
        /// Gets the collection of <see cref="Entity"/>s in this <see cref="Faction"/>.
        /// </summary>
        public IEnumerable<Unit> Units
        {
            get
            {
                return world.Entities
                    .OfType<Unit>()
                    .Where(unit => FactionMembership.GetFaction(unit) == this);
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="Entity"/>s in this <see cref="Faction"/>.
        /// </summary>
        public IEnumerable<Entity> Entities
        {
            get
            {
                return world.Entities
                    .Where(unit => FactionMembership.GetFaction(unit) == this);
            }
        }

        public IEnumerable<Technology> Technologies
        {
            get { return technologies; }
        }

        /// <summary>
        /// Gets the status of this faction.
        /// </summary>
        public FactionStatus Status
        {
            get { return status; }
        }

        #region Resources
        /// <summary>
        /// Accesses the amount of the aladdium resource that this <see cref="Faction"/> possesses.
        /// </summary>
        public int AladdiumAmount
        {
            get { return aladdiumAmount; }
            set
            {
                if (value == aladdiumAmount) return;
                Argument.EnsurePositive(value, "AladdiumAmount");
                aladdiumAmount = value;
                AladdiumAmountChanged.Raise(this);
            }
        }

        /// <summary>
        /// Accesses the amount of the alagene resource that this <see cref="Faction"/> possesses.
        /// </summary>
        public int AlageneAmount
        {
            get { return alageneAmount; }
            set
            {
                if (value == alageneAmount) return;
                Argument.EnsurePositive(value, "AlageneAmount");
                alageneAmount = value;
                AlageneAmountChanged.Raise(this);
            }
        }

        public int MaxFoodAmount
        {
            get { return Math.Max(Math.Min(world.MaximumFoodAmount, totalFoodAmount), minimumFoodAmount); }
        }

        private int TotalFoodAmount
        {
            get { return totalFoodAmount; }
            set
            {
                int previousMaxFoodAmount = MaxFoodAmount;
                totalFoodAmount = value;
                if (MaxFoodAmount != previousMaxFoodAmount)
                    MaxFoodAmountChanged.Raise(this);
            }
        }

        /// <summary>
        /// Gets the amount of food this <see cref="Faction"/> has not used.
        /// </summary>
        public int RemainingFoodAmount
        {
            get { return MaxFoodAmount - usedFoodAmount; }
        }

        /// <summary>
        /// Accesses the amount of food currently used by this <see cref="Faction"/>'s units.
        /// </summary>
        public int UsedFoodAmount
        {
            get { return usedFoodAmount; }
            set
            {
                if (value == usedFoodAmount) return;
                usedFoodAmount = value;
                UsedFoodAmountChanged.Raise(this);
        }
        }
        #endregion
        #endregion

        #region Methods
        #region Stats & Technologies
        /// <summary>
        /// Gets the value of a <see cref="Stat"/> which take researched technologies into account
        /// for a unit of this <see cref="Faction"/> by its <see cref="Entity"/>.
        /// </summary>
        /// <param name="prototype">The <see cref="Entity"/> for which the stat is to be retrieved.</param>
        /// <param name="stat">The <see cref="Stat"/> to be retrieved.</param>
        /// <returns>The value of that stat for the specified <see cref="Entity"/>.</returns>
        public StatValue GetStat(Entity prototype, Stat stat)
        {
            Argument.EnsureNotNull(prototype, "type");
            return prototype.GetStatValue(stat) + GetTechnologyBonuses(prototype, stat);
        }

        /// <summary>
        /// Gets the sum of the bonuses researched technologies offer to a stat.
        /// </summary>
        /// <param name="stat">The stat type.</param>
        /// <returns>The sum of the bonuses offered by technologies</returns>
        public StatValue GetTechnologyBonuses(Entity prototype, Stat stat)
        {
            StatValue sum = StatValue.CreateZero(stat.Type);
            foreach (Technology technology in technologies)
                sum += technology.GetEffect(prototype, stat);
            return sum;
        }

        public bool IsResearchable(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            return !HasResearched(technology)
                && !IsResearching(technology);
        }

        /// <summary>
        /// Adds a <see cref="Technology"/>,
        /// to the collection of technologies researched by this <see cref="Faction"/>.
        /// </summary>
        /// <param name="technology">The <see cref="Technology"/> to be researched.</param>
        public void AddResearchedTechnology(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            if (HasResearched(technology)) return;

            technologies.Add(technology);
            TechnologyResearched.Raise(this, technology);
        }

        public bool HasResearched(Technology technology)
        {
            return technologies.Contains(technology);
        }

        public bool IsResearching(Technology technology)
        {
            return researches.Contains(technology);
        }

        internal void BeginResearch(Technology technology)
        {
            Debug.Assert(technology != null);
            Debug.Assert(!HasResearched(technology));
            Debug.Assert(!researches.Contains(technology));
            Debug.Assert(IsResearchable(technology));
            researches.Add(technology);
        }

        internal void CancelResearch(Technology technology)
        {
            Debug.Assert(technology != null);
            bool wasRemoved = researches.Remove(technology);
            Debug.Assert(wasRemoved);
        }

        internal void CompleteResearch(Technology technology)
        {
            Debug.Assert(technology != null);
            bool wasRemoved = researches.Remove(technology);
            Debug.Assert(wasRemoved);
            technologies.Add(technology);
            TechnologyResearched.Raise(this, technology);
            RaiseWarning("La technologie {0} est maintenant disponible.".FormatInvariant(technology.Name));
        }
        #endregion

        #region Units
        /// <summary>
        /// Creates new <see cref="Entity"/> part of this <see cref="Faction"/>.
        /// </summary>
        /// <param name="prototype">The prototype of the <see cref="Entity"/> to be created.</param>
        /// <param name="point">The initial position of the <see cref="Entity"/>.</param>
        /// <returns>The newly created <see cref="Entity"/>.</returns>
        public Entity CreateUnit(Entity prototype, Point point)
        {
            Argument.EnsureNotNull(prototype, "prototype");

            Entity entity = world.Entities.CreateUnit(prototype, this, point);
            entity.Spatial.Moved += (s, oldPos, newPos) => OnEntityMoved(entity, oldPos, newPos);
            UsedFoodAmount += (int)GetStat(prototype, FactionMembership.FoodCostStat);

            return entity;
        }

        #region Entity notifications
        /// <remarks>Invoked by <see cref="Spatial"/>.</remarks>
        internal void OnEntityMoved(Entity entity, Vector2 oldPosition, Vector2 newPosition)
        {
            Debug.Assert(entity != null);
            Debug.Assert(FactionMembership.GetFaction(entity) == this);

            int sightRange = (int)entity.GetStatValue(Vision.RangeStat);
            Vector2 extent = entity.BoundingRectangle.Extent;
            Circle oldLineOfSight = new Circle(oldPosition + extent, sightRange);
            Circle newLineOfSight = new Circle(newPosition + extent, sightRange);
            localFogOfWar.UpdateLineOfSight(oldLineOfSight, newLineOfSight);
        }

        /// <remarks>Invoked by <see cref="Entity"/>.</remarks>
        internal void OnEntityDied(Entity entity)
        {
            Debug.Assert(entity != null);
            Debug.Assert(FactionMembership.GetFaction(entity) == this);

            UsedFoodAmount -= (int)GetStat(entity, FactionMembership.FoodCostStat);
        }

        /// <remarks>Invoked by <see cref="Entity"/>.</remarks>
        internal void OnUnitTypeChanged(Entity entity, Entity oldPrototype, Entity newPrototype)
        {
            Debug.Assert(entity != null);
            Debug.Assert(FactionMembership.GetFaction(entity) == this);
            Debug.Assert(oldPrototype != null);
            Debug.Assert(newPrototype != null);
            Debug.Assert(oldPrototype != newPrototype);

            UsedFoodAmount -= (int)GetStat(oldPrototype, FactionMembership.FoodCostStat);
            UsedFoodAmount += (int)GetStat(newPrototype, FactionMembership.FoodCostStat);

            if (entity.Components.Has<BuildProgress>())
            {
                Region oldRegion = Entity.GetGridRegion(entity.Position, oldPrototype.Size);
                Region newRegion = Entity.GetGridRegion(entity.Position, newPrototype.Size);
                localFogOfWar.RemoveRegion(oldRegion);
                localFogOfWar.AddRegion(newRegion);
            }
            else
            {
                Vector2 oldCenter = entity.Position + (Vector2)oldPrototype.Size * 0.5f;
                int oldSightRange = (int)GetStat(oldPrototype, Vision.RangeStat);
                
                Vector2 newCenter = entity.Position + (Vector2)newPrototype.Size * 0.5f;
                int newSightRange = (int)GetStat(newPrototype, Vision.RangeStat);

                localFogOfWar.RemoveLineOfSight(new Circle(oldCenter, oldSightRange));
                localFogOfWar.AddLineOfSight(new Circle(newCenter, newSightRange));
            }
        }

        /// <remarks>Invoked by <see cref="Entity"/>.</remarks>
        internal void OnBuildingConstructionCompleted(Entity entity)
        {
            Debug.Assert(entity != null);
            Debug.Assert(FactionMembership.GetFaction(entity) == this);

            localFogOfWar.RemoveRegion(entity.GridRegion);
            localFogOfWar.AddLineOfSight(entity.Components.Get<Vision>().LineOfSight);

            TotalFoodAmount += (int)entity.GetStatValue(FactionMembership.ProvidedFoodStat);
        }
        #endregion

        private void OnWorldEntityRemoved(World world, Entity entity)
        {
            if (FactionMembership.GetFaction(entity) != this) return;

            if (entity.Components.Has<BuildProgress>())
            {
                localFogOfWar.RemoveRegion(entity.Spatial.GridRegion);
            }
            else
            {
                TotalFoodAmount -= (int)entity.GetStatValue(FactionMembership.ProvidedFoodStat);
                localFogOfWar.RemoveLineOfSight(entity.Components.Get<Vision>().LineOfSight);
            }
        }

        private void OnWorldEntityAdded(World world, Entity entity)
        {
            if (FactionMembership.GetFaction(entity) != this) return;

            if (entity.Components.Has<BuildProgress>())
                localFogOfWar.AddRegion(entity.Spatial.GridRegion);
            else if (entity.Components.Has<Vision>())
                localFogOfWar.AddLineOfSight(entity.Components.Get<Vision>().LineOfSight);
        }

        /// <summary>
        /// Suicides all units in this faction.
        /// </summary>
        public void MassSuicide()
        {
            Entities.Select(entity => entity.Components.TryGet<Health>())
                .Where(health => health != null)
                .NonDeferred()
                .ForEach(entity => entity.Suicide());
        }

        /// <summary>
        /// Tests if a given resource node can be harvested by harvesters of this faction.
        /// </summary>
        /// <param name="node">The resource node to be tested.</param>
        /// <returns><c>True</c> if the resource node can be harvested, <c>false</c> otherwise.</returns>
        public bool CanHarvest(Entity node)
        {
            Argument.EnsureNotNull(node, "node");

            Harvestable harvestable = node.Components.TryGet<Harvestable>();
            if (harvestable == null
                || harvestable.Type == ResourceType.Aladdium) return true;

            Vector2 location = node.Spatial.Position;
            Entity extractor = world.Entities.GetGroundEntityAt(Point.Truncate(location));
            if (extractor == null) return false;

            Faction extractorFaction = FactionMembership.GetFaction(extractor);
            return extractor != null
                && extractor.Components.Has<AlageneExtractor>()
                && !extractor.Components.Has<BuildProgress>()
                && extractorFaction != null
                && GetDiplomaticStance(extractorFaction).HasFlag(DiplomaticStance.AlliedVictory);
        }
        #endregion

        /// <summary>
        /// Marks this faction as defeated. Does not cause a mass suicide.
        /// </summary>
        public void MarkAsDefeated()
        {
            if (status == FactionStatus.Defeated) return;

            status = FactionStatus.Defeated;
            Defeated.Raise(this);
            World.OnFactionDefeated(this);
        }

        #region Diplomacy
        /// <summary>
        /// Changes the diplomatic stance of this <see cref="Faction"/>
        /// in regard to another <see cref="Faction"/>.
        /// </summary>
        /// <param name="target">
        /// The <see cref="Faction"/> with which the diplomatic stance is to be changed.
        /// </param>
        /// <param name="stance">The new <see cref="DiplomaticStance"/> against that faction.</param>
        public void SetDiplomaticStance(Faction target, DiplomaticStance stance)
        {
            Argument.EnsureNotNull(target, "target");
            Argument.EnsurePossibleValue(stance, "stance");
            if (target == this) throw new ArgumentException("Cannot change the diplomatic stance against oneself.");

            if (GetDiplomaticStance(target).HasFlag(DiplomaticStance.ForeverAllied)
                && target.GetDiplomaticStance(target).HasFlag(DiplomaticStance.ForeverAllied))
                throw new InvalidOperationException("Cannot change the diplomatic stance once Shared Control has been set");

            DiplomaticStance previousStance = GetDiplomaticStance(target);
            DiplomaticStance otherFactionStance = target.GetDiplomaticStance(this);
            
            diplomaticStances[target] = stance;
            
            DiplomaticStanceChange change = new DiplomaticStanceChange(this, target, previousStance, stance);
            DiplomaticStanceChanged.Raise(this, change);
            target.OnOtherFactionDiplomaticStanceChanged(this, stance);
        }

        /// <summary>
        /// Gets the diplomatic stance of this <see cref="Faction"/>
        /// with regard to another one.
        /// </summary>
        /// <param name="faction">
        /// The <see cref="Faction"/> in regard to which the diplomatic stance is to be retrieved.
        /// </param>
        /// <returns>The <see cref="DiplomaticStance"/> with regard to that faction.</returns>
        public DiplomaticStance GetDiplomaticStance(Faction faction)
        {
            Debug.Assert(faction != null);

            DiplomaticStance diplomaticStance;
            return diplomaticStances.TryGetValue(faction, out diplomaticStance)
                ? diplomaticStance : DiplomaticStance.Enemy;
        }

        private void OnOtherFactionDiplomaticStanceChanged(Faction source, DiplomaticStance stance)
        {
            DiplomaticStance currentStance = GetDiplomaticStance(source);

            if (stance.HasFlag(DiplomaticStance.SharedVision))
            {
                source.LocalFogOfWar.Changed += fogOfWarChangedEventHandler;
                DiscoverFromOtherFogOfWar(source.localFogOfWar, (Region)world.Size);
            }
            else
            {
                source.LocalFogOfWar.Changed -= fogOfWarChangedEventHandler;
                if (currentStance.HasFlag(DiplomaticStance.SharedVision))
                    SetDiplomaticStance(source, currentStance.Exclude(DiplomaticStance.SharedVision));
            }

            if (!stance.HasFlag(DiplomaticStance.AlliedVictory) && currentStance.HasFlag(DiplomaticStance.AlliedVictory))
            {
                SetDiplomaticStance(source, currentStance.Exclude(DiplomaticStance.AlliedVictory));
            }

            // Invalidate the whole visibility to take into account new allies.
            VisibilityChanged.Raise(this, (Region)world.Size);
        }
        #endregion

        #region FogOfWar
        public bool HasPartiallySeen(Region region)
        {
            for (int offsetY = 0; offsetY < region.Height; ++offsetY)
            {
                for (int offsetX = 0; offsetX < region.Width; ++offsetX)
                {
                    Point point = new Point(region.MinX + offsetX, region.MinY + offsetY);
                    if (GetTileVisibility(point) != TileVisibility.Undiscovered)
                        return true;
                }
            }

            return false;
        }

        public bool HasFullySeen(Region region)
        {
            for (int offsetY = 0; offsetY < region.Height; ++offsetY)
            {
                for (int offsetX = 0; offsetX < region.Width; ++offsetX)
                {
                    Point point = new Point(region.MinX + offsetX, region.MinY + offsetY);
                    if (GetTileVisibility(point) == TileVisibility.Undiscovered)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tests if a <see cref="Region"/> of the world is at least partially visible.
        /// </summary>
        /// <param name="region">The region to be checked.</param>
        /// <returns>A value indicating if that region is at least partially visible.</returns>
        public bool CanSee(Region region)
        {
            for (int offsetY = 0; offsetY < region.Height; ++offsetY)
            {
                for (int offsetX = 0; offsetX < region.Width; ++offsetX)
                {
                    Point point = new Point(region.MinX + offsetX, region.MinY + offsetY);
                    if (GetTileVisibility(point) == TileVisibility.Visible)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests if an entity is visible by this <see cref="Faction"/>.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be tested.</param>
        /// <returns>A value indicating if it is visible.</returns>
        public bool CanSee(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            // Early out for units of our faction, which we can always see.
            if (FactionMembership.GetFaction(entity) == this) return true;

            Spatial spatial = entity.Spatial;
            return spatial != null && CanSee(spatial.GridRegion);
        }

        /// <summary>
        /// Gets the <see cref="TileVisibility"/> which indicates if this <see cref="Faction"/>
        /// currently sees a given tile.
        /// </summary>
        /// <param name="point">The coordinates of the tile.</param>
        /// <returns>The visibility of that tile.</returns>
        /// <remarks>
        /// This method is performance critical as it is called by the pathfinder.
        /// It should make no allocations and run as fast as possible.
        /// </remarks>
        public TileVisibility GetTileVisibility(Point point)
        {
            TileVisibility visibility = localFogOfWar.GetTileVisibility(point);
            if (visibility == TileVisibility.Visible) return TileVisibility.Visible;

            foreach (var pair in diplomaticStances)
            {
                Faction faction = pair.Key;
                if (!faction.GetDiplomaticStance(this).HasFlag(DiplomaticStance.SharedVision))
                    continue;

                if (faction.localFogOfWar.GetTileVisibility(point) == TileVisibility.Visible)
                    return TileVisibility.Visible;
                else if (faction.localFogOfWar.GetTileVisibility(point) == TileVisibility.Discovered)
                    visibility = TileVisibility.Discovered;
            }

            return visibility;
        }

        /// <summary>
        /// Tests if a point has been seen by this faction, without making out-of-bounds checks.
        /// </summary>
        /// <param name="point">The point to be tested.</param>
        /// <returns>A value indicating if that tile has been seen.</returns>
        /// <remarks>
        /// This method is performance critical as it is called by the pathfinder.
        /// It should make no allocations and run as fast as possible.
        /// </remarks>
        public bool HasSeen(Point point)
        {
            if (localFogOfWar.IsDiscovered(point)) return true;

            foreach (var pair in diplomaticStances)
            {
                Faction faction = pair.Key;
                if (!faction.GetDiplomaticStance(this).HasFlag(DiplomaticStance.SharedVision))
                    continue;

                if (faction.localFogOfWar.IsDiscovered(point))
                    return true;
            }

            return false;
        }

        private void OnFogOfWarChanged(FogOfWar sender, Region region)
        {
            if (sender != localFogOfWar)
            {
                // Another faction's fog of war was updated, the same regions should be discovered here,
                // otherwise when the faction is an enemy again it'll be as if we never discovered those places.
                DiscoverFromOtherFogOfWar(sender, region);
            }

            VisibilityChanged.Raise(this, region);
        }

        private void DiscoverFromOtherFogOfWar(FogOfWar other, Region region)
        {
            foreach (Point point in region.Points)
                if (other.GetTileVisibility(point) != TileVisibility.Undiscovered)
                    localFogOfWar.RevealWithoutRaisingEvent(point);
        }
        #endregion

        public override string ToString()
        {
            return name;
        }
        #endregion
        #endregion
    }
}
