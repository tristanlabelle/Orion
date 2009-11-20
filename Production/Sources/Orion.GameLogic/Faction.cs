using System;
using System.Collections.Generic;
using System.Linq;

using Color = System.Drawing.Color;
using OpenTK.Math;
using Orion.Geometry;
using Orion.GameLogic.Tasks;
using Orion.GameLogic.Pathfinding;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents a faction, a group of allied units sharing resources and sharing a goal.
    /// </summary>
    [Serializable]
    public sealed class Faction
    {
        #region Fields
        private readonly Handle handle;
        private readonly World world;
        private readonly string name;
        private readonly Color color;
        private readonly FogOfWar fogOfWar;
        private readonly ValueChangedEventHandler<Entity, Vector2> entityMovedEventHandler;
        private readonly GenericEventHandler<Entity> entityDiedEventHandler;
        private readonly HashSet<Faction> allies = new HashSet<Faction>();
        private readonly Pathfinder pathfinder;
        private int aladdiumAmount;
        private int alageneAmount;
        private const int maxFoodStock = 200;
        private int totalFoodStock = 0;
        private int usedFoodStock = 0;
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
            this.fogOfWar = new FogOfWar(world.Width, world.Height);
            this.entityMovedEventHandler = OnEntityMoved;
            this.entityDiedEventHandler = OnEntityDied;
            this.pathfinder = new Pathfinder(world.Width, world.Height, IsPathable);
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        /// <summary>
        /// Gets the handle of this <see cref="Faction"/>.
        /// </summary>
        public Handle Handle
        {
            get { return handle; }
        }

        /// <summary>
        /// Gets the <see cref="World"/> which hosts this faction.
        /// </summary>
        public World World
        {
            get { return world; }
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
                Argument.EnsurePositive(value, "AllageneAmount");
                alageneAmount = value;
            }
        }

        public Pathfinder PathFinder
        {
            get { return pathfinder; }
        }
        #endregion

        public int AvailableFood
        {

            get { return (Math.Min((maxFoodStock - usedFoodStock), (totalFoodStock - usedFoodStock))); }
        }

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

        #region Units
        /// <summary>
        /// Creates new <see cref="Unit"/> part of this <see cref="Faction"/>.
        /// </summary>
        /// <param name="type">The <see cref="UnitType"/> of the <see cref="Unit"/> to be created.</param>
        /// <param name="position">The initial position of the <see cref="Unit"/>.</param>
        /// <returns>The newly created <see cref="Unit"/>.</returns>
        public Unit CreateUnit(UnitType type, Vector2 position)
        {
            Unit unit = world.Entities.CreateUnit(type, this, position);

            if (type.HasSkill<Skills.ExtractAlagene>())
            {
                ResourceNode alageneNode = World.Entities
                                .OfType<ResourceNode>()
                                .First(node => node.Position == position
                                && node.Type == ResourceType.Alagene);

                alageneNode.Extractor = unit;
            }
            unit.Moved += entityMovedEventHandler;
            unit.Died += entityDiedEventHandler;
            fogOfWar.AddLineOfSight(unit.LineOfSight);
            usedFoodStock += type.FoodCost;
            if (unit.Type.HasSkill<Skills.StoreResources>())
                totalFoodStock += unit.Type.GetBaseStat(UnitStat.FoodStorageCapacity);
            return unit;
        }

        private void OnEntityMoved(Entity entity, ValueChangedEventArgs<Vector2> eventArgs)
        {
            Argument.EnsureBaseType(entity, typeof(Unit), "entity");

            Unit unit = (Unit)entity;
            float sightRange = unit.GetStat(UnitStat.SightRange);
            Circle oldLineOfSight = new Circle(eventArgs.OldValue, sightRange);
            Circle newLineOfSight = new Circle(eventArgs.NewValue, sightRange);
            fogOfWar.UpdateLineOfSight(oldLineOfSight, newLineOfSight);
        }

        private void OnEntityDied(Entity entity)
        {
            Argument.EnsureBaseType(entity, typeof(Unit), "entity");

            Unit unit = (Unit)entity;
            fogOfWar.RemoveLineOfSight(unit.LineOfSight);
            usedFoodStock -= unit.Type.FoodCost;
            if (unit.Type.HasSkill<Skills.StoreResources>())
                totalFoodStock -= unit.Type.GetBaseStat(UnitStat.FoodStorageCapacity);
            unit.Died -= entityDiedEventHandler;         
        }
        #endregion

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

        private bool IsPathable(int x, int y)
        {
            if (!world.IsWithinBounds(new Vector2(x, y)))
                return false;
            if (fogOfWar.GetTileVisibility(x, y) == TileVisibility.Undiscovered)
                return true;
            return world.Terrain.IsWalkable(x, y);
        }
        #endregion
    }
}
