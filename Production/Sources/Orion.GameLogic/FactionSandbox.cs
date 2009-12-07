using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.GameLogic
{
    /// <summary>
    /// Provides a sandboxed interface to the game <see cref="World"/>
    /// with the viewpoint of a specific <see cref="Faction"/>.
    /// </summary>
    public sealed class FactionSandbox
    {
        #region Fields
        private readonly Faction faction;
        #endregion

        #region Constructors
        public FactionSandbox(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");
            this.faction = faction;
            this.faction.World.Entities.Removed += OnEntityDied;
            this.faction.World.FactionDefeated += OnFactionDefeated;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when one entity of the world dies.
        /// </summary>
        public event GenericEventHandler<FactionSandbox, Handle> EntityDied;

        /// <summary>
        /// Raised when a faction gets defeated.
        /// </summary>
        public event GenericEventHandler<FactionSandbox, Handle> FactionDefeated;

        private void RaiseEntityDied(Handle handle)
        {
            var handler = EntityDied;
            if (handler != null) handler(this, handle);
        }

        private void RaiseFactionDefeated(Handle handle)
        {
            var handler = FactionDefeated;
            if (handler != null) handler(this, handle);
        }
        #endregion

        #region Properties
        #region Faction Accessors
        /// <summary>
        /// Gets the handle of the faction which's viewpoint is encapsulated in this sandbox.
        /// </summary>
        public Handle Faction
        {
            get { return faction.Handle; }
        }

        /// <summary>
        /// Gets the name of this faction.
        /// </summary>
        public string Name
        {
            get { return faction.Name; }
        }

        /// <summary>
        /// Gets the color of this faction.
        /// </summary>
        public Color Color
        {
            get { return faction.Color; }
        }

        /// <summary>
        /// Gets the amount of aladdium this faction possesses.
        /// </summary>
        public int AladdiumAmount
        {
            get { return faction.AladdiumAmount; }
        }

        /// <summary>
        /// Gets the amount of alagene this faction possesses.
        /// </summary>
        public int AlageneAmount
        {
            get { return faction.AlageneAmount; }
        }

        /// <summary>
        /// Gets the maximum amount of food available to this faction.
        /// </summary>
        public int MaxFoodAmount
        {
            get { return faction.MaxFoodAmount; }
        }

        /// <summary>
        /// Gets the amount of food that is used by this faction.
        /// </summary>
        public int UsedFoodAmount
        {
            get { return faction.UsedFoodAmount; }
        }

        /// <summary>
        /// Gets the amount of food that is available to be used by this faction.
        /// </summary>
        public int RemainingFoodAmount
        {
            get { return faction.RemainingFoodAmount; }
        }
        #endregion

        #region World Accessors
        /// <summary>
        /// Gets the size of the world, in tiles.
        /// </summary>
        public Size WorldSize
        {
            get { return faction.World.Size; }
        }

        /// <summary>
        /// Gets the unit types which can be used in this world.
        /// </summary>
        public IEnumerable<UnitType> UnitTypes
        {
            get { return faction.World.UnitTypes; }
        }

        /// <summary>
        /// Gets the sequence of handles to the factions of the world.
        /// </summary>
        public IEnumerable<Handle> Factions
        {
            get { return faction.World.Factions.Select(f => f.Handle); }
        }

        /// <summary>
        /// Gets the sequence of handles to the sandboxed faction's units.
        /// </summary>
        public IEnumerable<Handle> FactionUnits
        {
            get { return faction.Units.Select(unit => unit.Handle); }
        }

        /// <summary>
        /// Gets the sequence of handles to the units of this world
        /// which are visible by the sandboxed faction.
        /// </summary>
        public IEnumerable<Handle> VisibleWorldUnits
        {
            get
            {
                return faction.World.Entities
                    .OfType<Unit>()
                    .Where(unit => faction.GetTileVisibility((Point)unit.Position) == TileVisibility.Visible)
                    .Select(unit => unit.Handle);
            }
        }

        /// <summary>
        /// Gets the sequence of handles to the resource nodes of this world
        /// which are visible by the sandboxed faction.
        /// </summary>
        public IEnumerable<Handle> VisibleResourceNodes
        {
            get
            {
                return faction.World.Entities
                    .OfType<ResourceNode>()
                    .Where(node => faction.GetTileVisibility((Point)node.Position) == TileVisibility.Visible)
                    .Select(node => node.Handle);
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Public Getters
        #region Terrain
        public bool? IsTerrainWalkable(Point point)
        {
            if (faction.GetTileVisibility(point) == TileVisibility.Undiscovered)
                return null;
            return faction.World.Terrain.IsWalkableAndWithinBounds(point);
        }
        #endregion

        #region Sandboxed Faction Getters
        public TileVisibility GetTileVisibility(Point point)
        {
            return faction.GetTileVisibility(point);
        }

        public DiplomaticStance GetDiplomaticStanceWith(Handle otherFaction)
        {
            return faction.GetDiplomaticStance(GetFaction(otherFaction));
        }
        #endregion

        #region Other Faction Getters
        public FactionStatus GetFactionStatus(Handle faction)
        {
            return GetFaction(faction).Status;
        }

        public string GetFactionName(Handle faction)
        {
            return GetFaction(faction).Name;
        }

        public Color GetFactionColor(Handle faction)
        {
            return GetFaction(faction).Color;
        }
        #endregion

        #region Entities
        public bool IsEntityValid(Handle entity)
        {
            return GetEntity(entity) != null;
        }

        public Rectangle GetEntityPosition(Handle entity)
        {
            return GetEntity(entity).BoundingRectangle;
        }

        public Rectangle GetEntityBoundingRectangle(Handle entity)
        {
            return GetEntity(entity).BoundingRectangle;
        }
        #endregion

        #region Units
        public UnitType GetUnitType(Handle unit)
        {
            return GetUnit(unit).Type;
        }

        public float GetUnitHealth(Handle unit)
        {
            return GetUnit(unit).Health;
        }

        public int GetUnitStat(Handle unit, UnitStat stat)
        {
            return GetUnit(unit).GetStat(stat);
        }

        public IEnumerable<Type> GetUnitTaskQueue(Handle unit)
        {
            return GetUnit(unit).TaskQueue.Select(task => task.GetType());
        }

        public Type GetUnitTask(Handle unit)
        {
            var task = GetUnit(unit).TaskQueue.Current;
            if (task == null) return null;
            return task.GetType();
        }
        #endregion
        #endregion

        #region Handle Resolution
        public Faction GetFaction(Handle handle)
        {
            return faction.World.FindFactionFromHandle(handle);
        }

        private Entity GetEntity(Handle handle)
        {
            Entity entity = faction.World.Entities.FromHandle(handle);
            if (entity == null) return null;
            if (faction.GetTileVisibility((Point)entity.Position) != TileVisibility.Visible)
                return null;
            return entity;
        }

        private Unit GetUnit(Handle handle)
        {
            return (Unit)GetEntity(handle);
        }
        #endregion

        #region Event Handlers
        private void OnEntityDied(EntityManager sender, Entity args)
        {
            RaiseEntityDied(args.Handle);
        }

        private void OnFactionDefeated(World sender, Faction args)
        {
            RaiseFactionDefeated(args.Handle);
        }
        #endregion
        #endregion
    }
}
