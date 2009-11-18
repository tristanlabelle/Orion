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
        private readonly int id;
        private readonly World world;
        private readonly string name;
        private readonly Color color;
        private readonly FogOfWar fogOfWar;
        private readonly ValueChangedEventHandler<Entity, Rectangle> entityBoundingRectangleChangedEventHandler;
        private readonly GenericEventHandler<Entity> entityDiedEventHandler;
        private int aladdiumAmount;
        private int alageneAmount;
        private List<int> alliesID = new List<int>();
        private Pathfinder pathfinder;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Faction"/> from its name and <see cref="Color"/>.
        /// </summary>
        /// <param name="id">The unique identifier of this <see cref="Faction"/>.</param>
        /// <param name="world">The <see cref="World"/> hosting this <see cref="Faction"/>.</param>
        /// <param name="name">The name of this <see cref="Faction"/>.</param>
        /// <param name="color">The distinctive <see cref="Color"/> of this <see cref="Faction"/>'s units.</param>
        internal Faction(int id, World world, string name, Color color)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNullNorBlank(name, "name");

            this.id = id;
            this.world = world;
            this.name = name;
            this.color = color;
            this.fogOfWar = new FogOfWar(world.Width, world.Height);
            this.entityBoundingRectangleChangedEventHandler = OnEntityBoundingRectangleChanged;
            this.entityDiedEventHandler = OnEntityDied;
            this.pathfinder = new Pathfinder(world.Width, world.Height, IsPathable);
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        /// <summary>
        /// Gets the unique identifier of this <see cref="Faction"/>.
        /// </summary>
        public int ID
        {
            get { return id; }
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

        public List<int> AlliesID
        {
            get { return alliesID; }
        }

        public Pathfinder PathFinder
        {
            get { return pathfinder; }
        }
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
            unit.BoundingRectangleChanged += entityBoundingRectangleChangedEventHandler;
            unit.Died += entityDiedEventHandler;
            fogOfWar.AddLineOfSight(unit.LineOfSight);
            
            return unit;
        }

        private void OnEntityBoundingRectangleChanged(Entity entity, ValueChangedEventArgs<Rectangle> eventArgs)
        {
            Argument.EnsureBaseType(entity, typeof(Unit), "entity");

            Unit unit = (Unit)entity;
            float sightRange = unit.GetStat(UnitStat.SightRange);
            Circle oldLineOfSight = new Circle(eventArgs.OldValue.Center, sightRange);
            Circle newLineOfSight = new Circle(eventArgs.NewValue.Center, sightRange);
            fogOfWar.UpdateLineOfSight(oldLineOfSight, newLineOfSight);
        }

        private void OnEntityDied(Entity entity)
        {
            Argument.EnsureBaseType(entity, typeof(Unit), "entity");

            Unit unit = (Unit)entity;
            fogOfWar.RemoveLineOfSight(unit.LineOfSight);
            unit.Died -= entityDiedEventHandler;
        }

        #region Diplomacy
        public void AddAlly(int factionID)
        {
            Argument.EnsureNotNull(factionID, "factionID");
            if (alliesID.Contains(factionID)) return;//throw new Exception("{0} is already an ally".FormatInvariant(faction));
            alliesID.Add(factionID);
        }

        public void AddAlly(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");
            AddAlly(faction.id);
        }

        public void AddEnemy(int factionID)
        {
            Argument.EnsureNotNull(factionID, "factionID");
            if (!alliesID.Contains(factionID)) return;//throw new NullReferenceException("impossible to disally to an enemy...Need to be an ally first!");

            alliesID.Remove(factionID);
        }

        public void AddEnemy(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");
            AddEnemy(faction.id);
        }

        public DiplomaticStance GetDiplomaticStance(int factionID)
        {
            return alliesID.Contains(factionID) ? DiplomaticStance.Ally : DiplomaticStance.Enemy;
        }

        public DiplomaticStance GetDiplomaticStance(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");
            return GetDiplomaticStance(faction.ID);
        }

        public bool IsEnemy(Unit unit)
        {
            return !alliesID.Contains(unit.Faction.ID) && this.id != unit.Faction.ID;
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
            /*if (fogOfWar.GetTileVisibility(x, y) == TileVisibility.Undiscovered)
                return true;*/
            return world.Terrain.IsWalkable(x, y);
        }
        #endregion
    }
}
