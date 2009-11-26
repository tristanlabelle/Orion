using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Tasks;
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
        private readonly FogOfWar fogOfWar;
        private readonly ValueChangedEventHandler<Entity, Vector2> entityMovedEventHandler;
        private readonly GenericEventHandler<Entity> entityDiedEventHandler;
        private readonly HashSet<Faction> allies = new HashSet<Faction>();
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
        /// <param name="color">The distinctive <see cref="Color"/> of this <see cref="Faction"/>'s units.</param>
        internal Faction(Handle handle, World world, string name, Color color)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNullNorBlank(name, "name");

            this.handle = handle;
            this.world = world;
            this.name = name;
            this.color = color;
            this.fogOfWar = new FogOfWar(world.Size);
            this.entityMovedEventHandler = OnEntityMoved;
            this.entityDiedEventHandler = OnEntityDied;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Faction"/> gets defeated.
        /// </summary>
        public event GenericEventHandler<Faction> Defeated;

        private void OnDefeated()
        {
            var handler = Defeated;
            if (handler != null) handler(this);
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

        public FogOfWar FogOfWar
        {
            get { return fogOfWar; }
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
            return type.GetBaseStat(stat);
        }

        /// <summary>
        /// Finds a path from a source to a destination.
        /// </summary>
        /// <param name="source">The position where the path should start.</param>
        /// <param name="destination">The position the path should reach.</param>
        /// <returns>The path that was found, or <c>null</c> if there is none.</returns>
        public Path FindPath(Vector2 source, Vector2 destination)
        {
            return world.FindPath(source, destination, IsPathable);
        }

        #region Units
        /// <summary>
        /// Creates new <see cref="Unit"/> part of this <see cref="Faction"/>.
        /// </summary>
        /// <param name="type">The <see cref="UnitType"/> of the <see cref="Unit"/> to be created.</param>
        /// <param name="point">The initial position of the <see cref="Unit"/>.</param>
        /// <returns>The newly created <see cref="Unit"/>.</returns>
        public Unit CreateUnit(UnitType type, Point point)
        {
            Unit unit = world.Entities.CreateUnit(type, this, point);

            if (type.HasSkill<Skills.ExtractAlagene>())
            {
                ResourceNode alageneNode = World.Entities
                                .OfType<ResourceNode>()
                                .First(node => node.Position == point
                                && node.Type == ResourceType.Alagene);

                alageneNode.Extractor = unit;
            }
            unit.Moved += entityMovedEventHandler;
            unit.Died += entityDiedEventHandler;
            fogOfWar.AddLineOfSight(unit.LineOfSight);
            usedFoodAmount += type.FoodCost;
            if (unit.Type.HasSkill<Skills.StoreFood>())
                totalFoodAmount += unit.Type.GetBaseStat(UnitStat.FoodStorageCapacity);
            return unit;
        }

        private void OnEntityMoved(Entity entity, ValueChangedEventArgs<Vector2> eventArgs)
        {
            Argument.EnsureBaseType(entity, typeof(Unit), "entity");

            Unit unit = (Unit)entity;
            int sightRange = unit.GetStat(UnitStat.SightRange);
            Circle oldLineOfSight = new Circle(eventArgs.OldValue, sightRange);
            Circle newLineOfSight = new Circle(eventArgs.NewValue, sightRange);
            fogOfWar.UpdateLineOfSight(oldLineOfSight, newLineOfSight);
        }

        private void OnEntityDied(Entity entity)
        {
            Argument.EnsureBaseType(entity, typeof(Unit), "entity");

            Unit unit = (Unit)entity;
            fogOfWar.RemoveLineOfSight(unit.LineOfSight);
            usedFoodAmount -= unit.Type.FoodCost;
            if (unit.Type.HasSkill<Skills.StoreResources>())
                totalFoodAmount -= unit.Type.GetBaseStat(UnitStat.FoodStorageCapacity);
            unit.Died -= entityDiedEventHandler;

            CheckForDefeat();
        }
        #endregion

        private void CheckForDefeat()
        {
            if (status == FactionStatus.Defeated) return;
            
            bool hasKeepAliveUnit = Units.Any(u => u.IsAlive && u.Type.KeepsFactionAlive);
            if (!hasKeepAliveUnit)
            {
                status = FactionStatus.Defeated;
                OnDefeated();
            }
        }

        #region Diplomacy
        /// <summary>
        /// Changes the diplomatic stance of this <see cref="Faction"/>
        /// in regard to another <see cref="Faction"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Faction"/> with which the diplomatic stance is to be changed.
        /// </param>
        /// <param name="stance">The new <see cref="DiplomaticStance"/> against that faction.</param>
        public void SetDiplomaticStance(Faction other, DiplomaticStance stance)
        {
            Argument.EnsureNotNull(other, "other");
            if (other == this) throw new ArgumentException("Cannot change the diplomatic stance against oneself.");
            Argument.EnsureDefined(stance, "stance");

            if (stance == DiplomaticStance.Ally)
                allies.Add(other);
            else
                allies.Remove(other);
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
        #endregion

        public override string ToString()
        {
            return name;
        }

        private bool IsPathable(Point point)
        {
            if (!world.IsWithinBounds(point))
                return false;
            if (fogOfWar.GetTileVisibility(point) == TileVisibility.Undiscovered)
                return true;
            return world.Terrain.IsWalkable(point);
        }
        #endregion
    }
}
