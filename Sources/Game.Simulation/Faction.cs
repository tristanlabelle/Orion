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

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Represents a faction, a group of allied units sharing resources and sharing a goal.
    /// </summary>
    [Serializable]
    public sealed class Faction
    {
        #region Fields
        private const int minimumPopulationLimit = 10;

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

            diplomaticStances[this] = DiplomaticStance.SharedControl;

            this.handle = handle;
            this.world = world;
            this.name = name;
            this.color = color;
            this.localFogOfWar = new FogOfWar(world.Size);

            this.fogOfWarChangedEventHandler = OnFogOfWarChanged;
            this.localFogOfWar.Changed += fogOfWarChangedEventHandler;
            this.world.EntityAdded += OnWorldEntityAdded;
            this.world.EntityRemoved += OnWorldEntityRemoved;
            this.world.FactionDefeated += OnFactionDefeated;
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

        public event Action<Faction, string> Warning;

        public void RaiseWarning(string message, Faction source)
        {
#warning Ugly hack, event sender should always be the event owner!
            Warning.Raise(source, message);
        }

        public void RaiseWarning(string message)
        {
            RaiseWarning(message, this);
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
        /// Gets the collection of <see cref="Unit"/>s in this <see cref="Faction"/>.
        /// </summary>
        public IEnumerable<Unit> Units
        {
            get
            {
                return world.Entities
                    .OfType<Unit>()
                    .Where(unit => unit.Faction == this);
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

        private IEnumerable<Faction> FactionsSharingTheirVision
        {
            get
            {
                foreach (KeyValuePair<Faction, DiplomaticStance> pair in diplomaticStances)
                {
                    if (pair.Value.HasFlag(DiplomaticStance.SharedVision))
                        yield return pair.Key;
                }
            }
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
                Argument.EnsurePositive(value, "AladdiumAmount");
                aladdiumAmount = value;
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
                Argument.EnsurePositive(value, "AlageneAmount");
                alageneAmount = value;
            }
        }

        public int MaxFoodAmount
        {
            get { return Math.Max(Math.Min(world.MaximumFoodAmount, totalFoodAmount), minimumPopulationLimit); }
        }

        public int RemainingFoodAmount
        {
            get { return MaxFoodAmount - usedFoodAmount; }
        }

        public int UsedFoodAmount
        {
            get { return usedFoodAmount; }
            set { usedFoodAmount = value; }
        }
        #endregion
        #endregion

        #region Methods
        #region Stats & Technologies
        /// <summary>
        /// Gets the value of a <see cref="UnitStat"/> which take researched technologies into account
        /// for a unit of this <see cref="Faction"/> by its <see cref="UnitType"/>.
        /// </summary>
        /// <param name="type">The <see cref="UnitType"/> of the unit for which the stat is to be retrieved.</param>
        /// <param name="stat">The <see cref="UnitStat"/> to be retrieved.</param>
        /// <returns>The value of that stat for the specified <see cref="UnitType"/>.</returns>
        public int GetStat(UnitType type, UnitStat stat)
        {
            Argument.EnsureNotNull(type, "type");
            return type.GetBaseStat(stat) + GetTechnologyBonuses(type, stat);
        }

        /// <summary>
        /// Gets the sum of the bonuses researched technologies offer to a stat.
        /// </summary>
        /// <param name="stat">The stat type.</param>
        /// <returns>The sum of the bonuses offered by technologies</returns>
        public int GetTechnologyBonuses(UnitType type, UnitStat stat)
        {
            int total = 0;
            foreach (Technology technology in technologies)
                total += technology.GetEffect(type, stat);
            return total;
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
        /// Creates new <see cref="Unit"/> part of this <see cref="Faction"/>.
        /// </summary>
        /// <param name="type">The <see cref="UnitType"/> of the <see cref="Unit"/> to be created.</param>
        /// <param name="point">The initial position of the <see cref="Unit"/>.</param>
        /// <returns>The newly created <see cref="Unit"/>.</returns>
        public Unit CreateUnit(UnitType type, Point point)
        {
            Argument.EnsureNotNull(type, "type");

            Unit unit = world.Entities.CreateUnit(type, this, point);

            usedFoodAmount += GetStat(type, BasicSkill.FoodCostStat);

            return unit;
        }

        #region Notifications invoked by Unit
        /// <remarks>Invoked by Unit.</remarks>
        internal void OnUnitMoved(Unit unit, Vector2 oldPosition, Vector2 newPosition)
        {
            Debug.Assert(unit != null);
            Debug.Assert(unit.Faction == this);
            Debug.Assert(!unit.IsBuilding);

            int sightRange = unit.GetStat(BasicSkill.SightRangeStat);
            Vector2 extent = unit.BoundingRectangle.Extent;
            Circle oldLineOfSight = new Circle(oldPosition + extent, sightRange);
            Circle newLineOfSight = new Circle(newPosition + extent, sightRange);
            localFogOfWar.UpdateLineOfSight(oldLineOfSight, newLineOfSight);
        }

        /// <remarks>Invoked by Unit.</remarks>
        internal void OnUnitDied(Unit unit)
        {
            Debug.Assert(unit != null);
            Debug.Assert(unit.Faction == this);

            if (unit.Type.HasSkill<ProvideFoodSkill>())
                totalFoodAmount -= unit.GetStat(ProvideFoodSkill.AmountStat);

            usedFoodAmount -= GetStat(unit.Type, BasicSkill.FoodCostStat);
        }

        /// <remarks>Invoked by Unit.</remarks>
        internal void OnUnitTypeChanged(Unit unit, UnitType oldType, UnitType newType)
        {
            Debug.Assert(unit != null);
            Debug.Assert(unit.Faction == this);
            Debug.Assert(oldType != null);
            Debug.Assert(newType != null);
            Debug.Assert(oldType != newType);

            UsedFoodAmount -= GetStat(oldType, BasicSkill.FoodCostStat);
            UsedFoodAmount += GetStat(newType, BasicSkill.FoodCostStat);

            if (unit.IsUnderConstruction)
            {
                Region oldRegion = Entity.GetGridRegion(unit.Position, oldType.Size);
                Region newRegion = Entity.GetGridRegion(unit.Position, newType.Size);
                localFogOfWar.RemoveRegion(oldRegion);
                localFogOfWar.AddRegion(newRegion);
            }
            else
            {
                Vector2 oldCenter = unit.Position + (Vector2)oldType.Size * 0.5f;
                int oldSightRange = GetStat(oldType, BasicSkill.SightRangeStat);
                
                Vector2 newCenter = unit.Position + (Vector2)newType.Size * 0.5f;
                int newSightRange = GetStat(newType, BasicSkill.SightRangeStat);

                localFogOfWar.RemoveLineOfSight(new Circle(oldCenter, oldSightRange));
                localFogOfWar.AddLineOfSight(new Circle(newCenter, newSightRange));
            }
        }

        /// <remarks>Invoked by Unit.</remarks>
        internal void OnBuildingConstructionCompleted(Unit unit)
        {
            Debug.Assert(unit != null);
            Debug.Assert(unit.Faction == this);
            Debug.Assert(unit.IsBuilding);

            localFogOfWar.RemoveRegion(unit.GridRegion);
            localFogOfWar.AddLineOfSight(unit.LineOfSight);

            if (unit.HasSkill<ProvideFoodSkill>())
                totalFoodAmount += unit.GetStat(ProvideFoodSkill.AmountStat);
        }
        #endregion

        private void OnWorldEntityRemoved(World world, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null || unit.Faction != this) return;

            if (unit.IsUnderConstruction)
            {
                localFogOfWar.RemoveRegion(unit.GridRegion);
            }
            else
            {
                if (unit.Type.HasSkill<ProvideFoodSkill>())
                    totalFoodAmount -= unit.GetStat(ProvideFoodSkill.AmountStat);

                localFogOfWar.RemoveLineOfSight(unit.LineOfSight);
            }
        }

        private void OnWorldEntityAdded(World world, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null || unit.Faction != this) return;

            if (unit.IsBuilding && unit.IsUnderConstruction)
                localFogOfWar.AddRegion(unit.GridRegion);
            else
                localFogOfWar.AddLineOfSight(unit.LineOfSight);
        }

        private void OnFactionDefeated(World world, Faction faction)
        { }

        /// <summary>
        /// Suicides all units in this faction.
        /// </summary>
        public void MassSuicide()
        {
            Unit[] units = Units.OfType<Unit>().ToArray();
            foreach (Unit unit in units)
                unit.Suicide();
        }

        /// <summary>
        /// Tests if a given resource node can be harvested by harvesters of this faction.
        /// </summary>
        /// <param name="node">The resource node to be tested.</param>
        /// <returns><c>True</c> if the resource node can be harvested, <c>false</c> otherwise.</returns>
        public bool CanHarvest(ResourceNode node)
        {
            Argument.EnsureNotNull(node, "node");

            if (node.Type == ResourceType.Aladdium) return true;

            Unit extractor = world.Entities.GetGroundEntityAt(node.Position) as Unit;
            return extractor != null
                && extractor.HasSkill<ExtractAlageneSkill>()
                && !extractor.IsUnderConstruction
                && GetDiplomaticStance(extractor.Faction).HasFlag(DiplomaticStance.AlliedVictory);
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
            Argument.EnsureDefined(stance, "stance");
            if (target == this) throw new ArgumentException("Cannot change the diplomatic stance against oneself.");

            if (GetDiplomaticStance(target).HasFlag(DiplomaticStance.SharedControl)
                && target.GetDiplomaticStance(target).HasFlag(DiplomaticStance.SharedControl))
                throw new InvalidOperationException("Cannot change the diplomatic stance once it's been set to Shared Control");

            DiplomaticStance previousStance = GetDiplomaticStance(target);
            DiplomaticStance otherFactionStance = target.GetDiplomaticStance(this);

            if (stance.HasFlag(DiplomaticStance.SharedControl))
            {
                target.RaiseWarning("{0} désire partager le contrôle avec vous.".FormatInvariant(this), this);
            }
            else
            {
                if (stance.HasFlag(DiplomaticStance.SharedVision) && !previousStance.HasFlag(DiplomaticStance.SharedVision))
                    target.RaiseWarning("{0} partage sa vision avec vous.".FormatInvariant(this), this);
                else if (!stance.HasFlag(DiplomaticStance.SharedVision) && previousStance.HasFlag(DiplomaticStance.SharedVision))
                    target.RaiseWarning("{0} ne partage plus sa vision avec vous.".FormatInvariant(this), this);


                if (stance.HasFlag(DiplomaticStance.AlliedVictory) && !previousStance.HasFlag(DiplomaticStance.AlliedVictory))
                    target.RaiseWarning("{0} désire partager la victoire avec vous.".FormatInvariant(this), this);
                else if (!stance.HasFlag(DiplomaticStance.AlliedVictory) && previousStance.HasFlag(DiplomaticStance.AlliedVictory))
                    target.RaiseWarning("{0} ne partagera plus la victoire avec vous.".FormatInvariant(this), this);
            }

            diplomaticStances[target] = stance;
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
            if (!diplomaticStances.ContainsKey(faction))
                diplomaticStances.Add(faction, DiplomaticStance.Enemy);
            return diplomaticStances[faction];
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
            return region.Points
                .Any(point => GetTileVisibility(point) != TileVisibility.Undiscovered);
        }

        public bool HasFullySeen(Region region)
        {
            return region.Points
                .All(point => GetTileVisibility(point) != TileVisibility.Undiscovered);
        }

        /// <summary>
        /// Tests if a <see cref="Region"/> of the world is at least partially visible.
        /// </summary>
        /// <param name="region">The region to be checked.</param>
        /// <returns>A value indicating if that region is at least partially visible.</returns>
        public bool CanSee(Region region)
        {
            for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
                for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
                    if (GetTileVisibility(new Point(x, y)) == TileVisibility.Visible)
                        return true;
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
            Unit unit = entity as Unit;
            if (unit != null && unit.Faction == this) return true;

            return CanSee(entity.GridRegion);
        }

        public TileVisibility GetTileVisibility(Point point)
        {
            TileVisibility visibility = localFogOfWar.GetTileVisibility(point);
            if (visibility == TileVisibility.Visible) return TileVisibility.Visible;

            foreach (Faction faction in FactionsSharingTheirVision)
            {
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
        public bool HasSeen(Point point)
        {
            if (localFogOfWar.IsDiscovered(point)) return true;

            foreach (Faction faction in FactionsSharingTheirVision)
                if (faction.localFogOfWar.IsDiscovered(point))
                    return true;

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
    }
}
