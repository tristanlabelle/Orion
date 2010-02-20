using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Tasks;
using Orion.GameLogic.Technologies;
using Orion.Geometry;
using Orion.GameLogic.Skills;
using ColorPalette = Orion.Colors;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents a faction, a group of allied units sharing resources and sharing a goal.
    /// </summary>
    [Serializable]
    public sealed class Faction
    {
        #region Fields
        private const int maxFoodAmount = 200;
        public static ColorRgb[] Colors = new ColorRgb[]
        {
            ColorPalette.Red, ColorPalette.Cyan, ColorPalette.Yellow, ColorPalette.Orange,
            ColorPalette.Green, ColorPalette.Pink, ColorPalette.Gray, ColorPalette.DarkBlue,
            ColorPalette.Lime, ColorPalette.Purple, ColorPalette.White, ColorPalette.Chocolate
        };

        private readonly Handle handle;
        private readonly World world;
        private readonly string name;
        private readonly ColorRgb color;
        private readonly FogOfWar localFogOfWar;

        private readonly HashSet<Faction> factionsWeRegardAsAllies = new HashSet<Faction>();
        private readonly HashSet<Faction> factionsRegardingUsAsAllies = new HashSet<Faction>();

        private readonly HashSet<Technology> researches = new HashSet<Technology>();
        private readonly HashSet<Technology> technologies = new HashSet<Technology>();

        private readonly Action<FogOfWar, Region> fogOfWarChangedEventHandler;
        private readonly ValueChangedEventHandler<Entity, Vector2> unitMovedEventHandler;
        private readonly Action<Unit> buildingConstructionCompleted;

        private int aladdiumAmount;
        private int alageneAmount;
        private int totalFoodAmount = 0;
        private int usedFoodAmount = 0;
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

            this.handle = handle;
            this.world = world;
            this.name = name;
            this.color = color;
            this.localFogOfWar = new FogOfWar(world.Size);
            this.fogOfWarChangedEventHandler = OnFogOfWarChanged;
            this.localFogOfWar.Changed += fogOfWarChangedEventHandler;
            this.unitMovedEventHandler = OnUnitMoved;
            this.buildingConstructionCompleted = OnBuildingConstructionCompleted;

            world.FactionDefeated += OnFactionDefeated;
            this.world.Entities.Removed += OnEntityRemoved;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Faction"/> gets defeated.
        /// </summary>
        public event Action<Faction> Defeated;

        private void RaiseDefeated()
        {
            var handler = Defeated;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Raised when the area of the world that is visible by this faction changes.
        /// </summary>
        public event Action<Faction, Region> VisibilityChanged;

        private void RaiseVisibilityChanged(Region region)
        {
            var handler = VisibilityChanged;
            if (handler != null) handler(this, region);
        }

        /// <summary>
        /// Raised when a new <see cref="Technology"/> has been researched.
        /// </summary>
        public event Action<Faction, Technology> TechnologyResearched;

        private void RaiseTechnologyResearched(Technology technology)
        {
            var handler = TechnologyResearched;
            if (handler != null) handler(this, technology);
        }

        public event Action<Faction, string> Warning;

        private void RaiseWarning(string message, Faction source)
        {
#warning Ugly hack, event sender should always be the event owner!

#if DEBUG
            // #if'd so the FormatInvariant is not executed in release.
            Debug.WriteLine("{0} faction warning: {1}".FormatInvariant(this, message));
#endif

            var handler = Warning;
            if (handler != null) handler(source, message);
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

        private bool IsStuck
        {
            get { return MaxFoodAmount == 0 && !Units.Any(u => u.HasSkill<Skills.BuildSkill>()); }
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
            get { return Math.Min(maxFoodAmount, totalFoodAmount); }
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
            return technologies.Sum(tech => tech.GetEffect(type, stat));
        }

        public bool IsResearchable(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            return !HasResearched(technology)
                && !IsResearching(technology)
                && technology.RequiredTechnologies.All(tech => technologies.Contains(tech));
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

            Technology firstMissingTechnology = technology.Requirements.Technologies
                .FirstOrDefault(t => !technologies.Contains(technology));
            if (firstMissingTechnology != null)
            {
                throw new InvalidOperationException(
                    "Cannot develop technology {0} without {1}."
                    .FormatInvariant(technology, firstMissingTechnology));
            }

#if DEBUG
            // #if'd for performance
            Debug.Assert(technology.Effects.All(effect => effect.Stat != UnitStat.SightRange && effect.Stat != UnitStat.FoodStorageCapacity),
                "Sight range and food storage capacity changing technologies are not supported, they would cause bugs.");
#endif

            technologies.Add(technology);
            RaiseTechnologyResearched(technology);
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
            RaiseTechnologyResearched(technology);
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

            if (type.HasSkill<Skills.ExtractAlageneSkill>())
            {
                ResourceNode alageneNode = World.Entities
                                .OfType<ResourceNode>()
                                .First(node => node.Position == point
                                && node.Type == ResourceType.Alagene);

                alageneNode.Extractor = unit;
            }

            if (unit.IsBuilding)
            {
                unit.ConstructionCompleted += buildingConstructionCompleted;
                localFogOfWar.AddRegion(unit.GridRegion);
            }
            else
            {
                localFogOfWar.AddLineOfSight(unit.LineOfSight);
            }

            unit.Moved += unitMovedEventHandler;
            usedFoodAmount += type.FoodCost;

            return unit;
        }

        private void OnUnitMoved(Entity entity, Vector2 oldPosition, Vector2 newPosition)
        {
            Unit unit = (Unit)entity;
            Debug.Assert(!unit.IsBuilding);

            int sightRange = unit.GetStat(UnitStat.SightRange);
            Vector2 extent = entity.BoundingRectangle.Extent;
            Circle oldLineOfSight = new Circle(oldPosition + extent, sightRange);
            Circle newLineOfSight = new Circle(newPosition + extent, sightRange);
            localFogOfWar.UpdateLineOfSight(oldLineOfSight, newLineOfSight);
        }

        private void OnEntityRemoved(EntityManager sender, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null || unit.Faction != this) return;

            if (unit.IsUnderConstruction)
            {
                unit.ConstructionCompleted -= buildingConstructionCompleted;
                localFogOfWar.RemoveRegion(unit.GridRegion);
            }
            else
            {
                if (unit.Type.HasSkill<Skills.StoreFoodSkill>())
                    totalFoodAmount -= unit.GetStat(UnitStat.FoodStorageCapacity);

                localFogOfWar.RemoveLineOfSight(unit.LineOfSight);
            }

            usedFoodAmount -= unit.Type.FoodCost;

            CheckForDefeat();
        }

        private void OnFactionDefeated(World world, Faction faction)
        {
            if(faction == this) return;
            factionsWeRegardAsAllies.Remove(faction);
        }

        private void OnBuildingConstructionCompleted(Unit unit)
        {
            Debug.Assert(unit != null && unit.Faction == this);

            unit.ConstructionCompleted -= buildingConstructionCompleted;
            localFogOfWar.RemoveRegion(unit.GridRegion);
            localFogOfWar.AddLineOfSight(unit.LineOfSight);

            if (unit.HasSkill<StoreFoodSkill>()) totalFoodAmount += unit.GetStat(UnitStat.FoodStorageCapacity);
        }
        #endregion

        public void GiveUp()
        {
            Unit[] units = Units.OfType<Unit>().ToArray();
            foreach (Unit unit in units)
                unit.Suicide();
        }

        private void CheckForDefeat()
        {
            if (status == FactionStatus.Defeated) return;

            bool hasKeepAliveUnit = Units.Any(u => u.IsAlive && u.Type.KeepsFactionAlive);
            if (!hasKeepAliveUnit || IsStuck)
            {
                status = FactionStatus.Defeated;
                RaiseDefeated();
                return;
            }
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
            if (target == this) throw new ArgumentException("Cannot change the diplomatic stance against oneself.");
            Argument.EnsureDefined(stance, "stance");

            if (factionsWeRegardAsAllies.Contains(target) == (stance == DiplomaticStance.Ally))
                return;

            if (stance == DiplomaticStance.Ally)
            {
                factionsWeRegardAsAllies.Add(target);
                target.RaiseWarning("{0} est maintenant votre allié.".FormatInvariant(this), this);
            }
            else
            {
                factionsWeRegardAsAllies.Remove(target);
                target.SetDiplomaticStance(this, DiplomaticStance.Enemy);
                target.RaiseWarning("{0} est maintenant hostile à votre égard.".FormatInvariant(this), this);
            }

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
            return faction == this || factionsWeRegardAsAllies.Contains(faction)
                ? DiplomaticStance.Ally : DiplomaticStance.Enemy;
        }

        private void OnOtherFactionDiplomaticStanceChanged(Faction source, DiplomaticStance stance)
        {
            if (stance == DiplomaticStance.Ally)
            {
                // As another faction has set us as allies, so we see what they do.
                source.localFogOfWar.Changed += fogOfWarChangedEventHandler;
                DiscoverFromOtherFogOfWar(source.localFogOfWar, (Region)world.Size);

                Debug.Assert(!factionsRegardingUsAsAllies.Contains(source));
                factionsRegardingUsAsAllies.Add(source);
            }
            else
            {
                source.localFogOfWar.Changed -= fogOfWarChangedEventHandler;

                Debug.Assert(factionsRegardingUsAsAllies.Contains(source));
                factionsRegardingUsAsAllies.Remove(source);
            }

            // Invalidate the whole visibility to take into account new allies.
            RaiseVisibilityChanged((Region)world.Size);
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

            foreach (Faction faction in factionsRegardingUsAsAllies)
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

            foreach (Faction faction in factionsRegardingUsAsAllies)
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

            RaiseVisibilityChanged(region);
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
