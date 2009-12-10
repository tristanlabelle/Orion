using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Tasks;
using Orion.GameLogic.Technologies;
using Orion.Geometry;
using Color = System.Drawing.Color;

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

        private readonly Handle handle;
        private readonly World world;
        private readonly string name;
        private readonly Color color;
        private readonly FogOfWar localFogOfWar;
        private readonly GenericEventHandler<FogOfWar, Region> fogOfWarChangedEventHandler;
        private readonly ValueChangedEventHandler<Entity, Vector2> unitMovedEventHandler;
        private readonly GenericEventHandler<EntityManager, Entity> entityRemovedEventHandler;
        private readonly GenericEventHandler<Unit> foodStorageCreated;
        private readonly HashSet<Faction> allies = new HashSet<Faction>();
        private readonly HashSet<RememberedBuilding> buildingMemory = new HashSet<RememberedBuilding>();
        private int aladdiumAmount;
        private int alageneAmount;
        private int totalFoodAmount = 0;
        private int usedFoodAmount = 0;
        private FactionStatus status = FactionStatus.Undefeated;
        private HashSet<Technology> technologies = new HashSet<Technology>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Faction"/> from its name and <see cref="Color"/>.
        /// </summary>
        /// <param name="handle">The handle of this <see cref="Faction"/>.</param>
        /// <param name="world">The <see cref="World"/> hosting this <see cref="Faction"/>.</param>
        /// <param name="name">The name of this <see cref="Faction"/>.</param>
        /// <param name="color">The distinctive <see cref="Color"/> of this <see cref="Faction"/>'s units.</param>
        internal Faction(Handle handle, World world, string name, Color color)
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
            this.entityRemovedEventHandler = OnEntityRemoved;
            this.foodStorageCreated = OnFoodStorageCreated;

            this.world.Entities.Removed += entityRemovedEventHandler;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Faction"/> gets defeated.
        /// </summary>
        public event GenericEventHandler<Faction> Defeated;

        private void RaiseDefeated()
        {
            var handler = Defeated;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Raised when the area of the world that is visible by this faction changes.
        /// </summary>
        public event GenericEventHandler<Faction, Region> VisibilityChanged;

        private void RaiseVisibilityChanged(Region region)
        {
            var handler = VisibilityChanged;
            if (handler != null) handler(this, region);
        }

        /// <summary>
        /// Raised when a new <see cref="Technology"/> has been researched.
        /// </summary>
        public event GenericEventHandler<Faction, Technology> TechnologyResearched;

        private void RaiseTechnologyResearched(Technology technology)
        {
            var handler = TechnologyResearched;
            if (handler != null) handler(this, technology);
        }

        public event GenericEventHandler<Faction,string> Warning;
        public void RaiseWarning(string message)
        {
            var handler = Warning;
            if (handler != null) handler(this, message);
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
        public Color Color
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
        /// Gets the building memory this faction has of other faction buildings.
        /// </summary>
        public IEnumerable<RememberedBuilding> BuildingMemory
        {
            get
            {
                return buildingMemory.Where(rememberedBuilding => !CanSee(rememberedBuilding.GridRegion));
            }
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
            return type.GetBaseStat(stat) + GetTechnologyBonuses(stat);
        }

        /// <summary>
        /// Gets the sum of the bonuses researched technologies offer to a stat.
        /// </summary>
        /// <param name="stat">The stat type.</param>
        /// <returns>The sum of the bonuses offered by technologies</returns>
        public int GetTechnologyBonuses(UnitStat stat)
        {
            return technologies
                .SelectMany(tech => tech.Effects)
                .Where(effect => effect.Stat == stat)
                .Sum(effect => effect.Value);
        }

        public bool IsResearchable(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            return technology.Requirements.Technologies == null || technology.Requirements.Technologies
                .All(tech => technologies.Contains(tech));
        }

        /// <summary>
        /// Adds a <see cref="Technology"/>,
        /// to the collection of technologies researched by this <see cref="Faction"/>.
        /// </summary>
        /// <param name="technology">The <see cref="Technology"/> to be researched.</param>
        public void ResearchTechnology(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            if (technologies.Contains(technology)) return;

            Technology firstMissingTechnology = technology.Requirements.Technologies
                .FirstOrDefault(t => !technologies.Contains(technology));
            if (firstMissingTechnology != null)
            {
                throw new InvalidOperationException(
                    "Cannot develop technology {0} without {1}."
                    .FormatInvariant(technology, firstMissingTechnology));
            }

            Debug.Assert(technology.Effects.All(effect => effect.Stat != UnitStat.SightRange && effect.Stat != UnitStat.FoodStorageCapacity),
                "Sight range and food storage capacity changing technologies are not supported, they would cause bugs.");

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

            localFogOfWar.AddLineOfSight(unit.LineOfSight);
            unit.Moved += unitMovedEventHandler;

            usedFoodAmount += type.FoodCost;

            if (unit.Type.HasSkill<Skills.StoreFoodSkill>())
                unit.ConstructionComplete += foodStorageCreated;

            return unit;
        }

        private void OnUnitMoved(Entity entity, Vector2 oldPosition, Vector2 newPosition)
        {
            Argument.EnsureBaseType(entity, typeof(Unit), "entity");

            Unit unit = (Unit)entity;
            int sightRange = unit.GetStat(UnitStat.SightRange);
            Vector2 extent = entity.BoundingRectangle.Extent;
            Circle oldLineOfSight = new Circle(oldPosition + extent, sightRange);
            Circle newLineOfSight = new Circle(newPosition + extent, sightRange);
            localFogOfWar.UpdateLineOfSight(oldLineOfSight, newLineOfSight);
        }

        private void OnEntityRemoved(EntityManager sender, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            if (unit.Faction == this)
            {
                localFogOfWar.RemoveLineOfSight(unit.LineOfSight);
                usedFoodAmount -= unit.Type.FoodCost;
                if (unit.Type.HasSkill<Skills.StoreFoodSkill>() && !unit.IsUnderConstruction)
                    totalFoodAmount -= unit.GetStat(UnitStat.FoodStorageCapacity);

                CheckForDefeat();
            }
            else if (unit.IsBuilding && CanSee(unit))
            {
                buildingMemory.Remove(new RememberedBuilding(unit));
            }
        }

        private void OnFoodStorageCreated(Unit unit)
        {
            unit.ConstructionComplete -= foodStorageCreated;
            totalFoodAmount += unit.GetStat(UnitStat.FoodStorageCapacity);
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
            if (!hasKeepAliveUnit)
            {
                status = FactionStatus.Defeated;
                RaiseDefeated();
                return;
            }
            if (IsStuck())
            {
                status = FactionStatus.Defeated;
                RaiseDefeated();
                return;
            }
        }
        
        private bool IsStuck()
        {
            if (MaxFoodAmount == 0 && !Units.Any(u => u.HasSkill<Skills.BuildSkill>()))
                return true;
            return false;
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

            if (allies.Contains(target) == (stance == DiplomaticStance.Ally))
                return;

            if (stance == DiplomaticStance.Ally)
                allies.Add(target);
            else
                allies.Remove(target);

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
            Argument.EnsureNotNull(faction, "faction");
            if (faction == this) return DiplomaticStance.Ally;
            return allies.Contains(faction) ? DiplomaticStance.Ally : DiplomaticStance.Enemy;
        }

        private void OnOtherFactionDiplomaticStanceChanged(Faction source, DiplomaticStance stance)
        {
            if (stance == DiplomaticStance.Ally)
            {
                // As another faction has set us as allies, so we see what they do.
                source.localFogOfWar.Changed += fogOfWarChangedEventHandler;
                DiscoverFromOtherFogOfWar(source.localFogOfWar, (Region)world.Size);
            }
            else
            {
                source.localFogOfWar.Changed -= fogOfWarChangedEventHandler;
            }

            // Invalidate the whole visibility to take into account new allies.
            RaiseVisibilityChanged((Region)world.Size);
        }
        #endregion

        #region FogOfWar
        public bool HasSeen(Region region)
        {
            for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
                for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
                    if (GetTileVisibility(new Point(x, y)) != TileVisibility.Undiscovered)
                        return true;
            return false;
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
            return CanSee(entity.GridRegion);
        }

        public TileVisibility GetTileVisibility(Point point)
        {
            TileVisibility visibility = localFogOfWar.GetTileVisibility(point);
            if (visibility == TileVisibility.Visible) return TileVisibility.Visible;

            foreach (Faction faction in world.Factions.Where(f => f.GetDiplomaticStance(this) == DiplomaticStance.Ally))
            {
                if (faction.localFogOfWar.GetTileVisibility(point) == TileVisibility.Visible)
                    return TileVisibility.Visible;
                else if (faction.localFogOfWar.GetTileVisibility(point) == TileVisibility.Discovered)
                    visibility = TileVisibility.Discovered;
            }

            return visibility;
        }

        private void OnFogOfWarChanged(FogOfWar sender, Region region)
        {
            if (sender != localFogOfWar)
            {
                // Another faction's fog of war was updated, the same regions should be discovered here,
                // otherwise when the faction is an enemy again it'll be as if we never discovered those places.
                DiscoverFromOtherFogOfWar(sender, region);
            }

            UpdateBuildingMemory(region);
            RaiseVisibilityChanged(region);
        }

        private void UpdateBuildingMemory(Region region)
        {
            var visibleOtherFactionBuildingsInRegion = world.Entities.InArea(region.ToRectangle())
                .OfType<Unit>()
                .Where(unit => unit.IsBuilding && unit.Faction != this && CanSee(unit))
                .Select(building => new RememberedBuilding(building));
            buildingMemory.UnionWith(visibleOtherFactionBuildingsInRegion);

            buildingMemory.RemoveWhere(rememberedBuilding =>
            {
                Unit building = world.Entities.GetEntityAt(rememberedBuilding.Location, CollisionLayer.Ground) as Unit;
                return building == null || !rememberedBuilding.Matches(building);
            });
        }

        private void DiscoverFromOtherFogOfWar(FogOfWar other, Region region)
        {
            foreach (Point point in region.Points)
                if (other.GetTileVisibility(point) != TileVisibility.Undiscovered)
                    localFogOfWar.Discover(point);
        }
        #endregion

        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
