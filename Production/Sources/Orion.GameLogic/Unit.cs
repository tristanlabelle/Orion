using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;
using System.Diagnostics;
using Orion.Geometry;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents an in-game unit, which can be a character, a vehicle or a building,
    /// depending on its <see cref="UnitType"/>.
    /// </summary>
    [Serializable]
    public sealed class Unit
    {
        #region Fields
        private readonly int id;
        private readonly UnitType type;
        internal Faction faction;

        /// <summary>
        /// The last position as stored in the <see cref="UnitRegistry"/>.
        /// Do not modify.
        /// </summary>
        internal Vector2 lastKnownPosition;

        private Vector2 position;
        private float angle;
        private float damage;
        private Task task = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Unit"/> from its identifier,
        /// <see cref="UnitType"/> and <see cref="World"/>.
        /// </summary>
        /// <param name="id">A hopefully unique identifier for this <see cref="Unit"/>.</param>
        /// <param name="type">
        /// The <see cref="UnitType"/> which determines
        /// the stats and capabilities of this <see cref="Unit"/>
        /// </param>
        /// <param name="faction">The <see cref="Faction"/> this <see cref="Unit"/> is part of.</param>
        internal Unit(int id, UnitType type, Faction faction)
        {
            Argument.EnsureNotNull(type, "type");
            Argument.EnsureNotNull(faction, "faction");

            this.id = id;
            this.type = type;
            this.faction = faction;
        }
        #endregion

        #region Events
        #region Moved
        /// <summary>
        /// Raised when this <see cref="Unit"/> moves.
        /// </summary>
        public event ValueChangedEventHandler<Unit, Vector2> Moved;

        private void OnMoved(Vector2 oldPosition, Vector2 newPosition)
        {
            if (Moved != null) Moved(this, new ValueChangedEventArgs<Vector2>(oldPosition, newPosition));
        }
        #endregion

        #region DamageChanged
        /// <summary>
        /// Raised when this <see cref="Unit"/> gets damaged or healed.
        /// </summary>
        public event GenericEventHandler<Unit> DamageChanged;

        private void OnDamageChanged()
        {
            if (DamageChanged != null) DamageChanged(this);
        }
        #endregion

        #region Died
        /// <summary>
        /// Raised when this <see cref="Unit"/> has died.
        /// </summary>
        public event GenericEventHandler<Unit> Died;

        private void OnDied()
        {
            if (Died != null) Died(this);
        }
        #endregion
        #endregion

        #region Properties
        #region Identification
        /// <summary>
        /// Gets the unique identifier of this <see cref="Unit"/>.
        /// </summary>
        public int ID
        {
            get { return id; }
        }

        /// <summary>
        /// Gets the <see cref="UnitType"/> of this <see cref="Unit"/>.
        /// </summary>
        public UnitType Type
        {
            get { return type; }
        }
        #endregion

        #region Affiliation
        /// <summary>
        /// Gets the <see cref="World"/> containing this <see cref="Unit"/>.
        /// </summary>
        public World World
        {
            get { return faction.World; }
        }

        /// <summary>
        /// Accesses the <see cref="Faction"/> which this <see cref="Unit"/> is a member of.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
            set
            {
                if (value == faction) return;

                if (faction != null)
                {
                    faction.Units.Remove(this);
                    Debug.Assert(faction == null,
                        "Removing a unit from a faction should have set its faction to null.");
                }

                if (value != null)
                {
                    value.Units.Add(this);
                    Debug.Assert(faction == value,
                        "Adding a unit to a faction should have set its faction that faction.");
                }
            }
        }
        #endregion

        #region State
        /// <summary>
        /// Accesses the position of this <see cref="Unit"/>, in <see cref="World"/> coordinates.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set
            {
                if (value == position) return;
                if (!World.Bounds.ContainsPoint(value))
                {
                    throw new ArgumentException(
                        "Cannot set the position to a value outside of world bounds.",
                        "Position");
                }

                Vector2 oldValue = position;
                position = value;
                OnMoved(oldValue, position);
            }
        }

        /// <summary>
        /// Gets the bounding <see cref="Circle"/> of this <see cref="Unit"/>.
        /// </summary>
        public Circle Circle
        {
            get { return new Circle(position, 0.5f); }
        }

        /// <summary>
        /// Gets the angle this <see cref="Unit"/> is facing.
        /// </summary>
        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        /// <summary>
        /// Gets a circle representing the area of the world that is within
        /// the line of sight of this <see cref="Unit"/>.
        /// </summary>
        public Circle LineOfSight
        {
            get { return new Circle(position, GetStat(UnitStat.SightRange)); }
        }

        #region Health
        /// <summary>
        /// Accesses the damage that has been inflicted to this <see cref="Unit"/>, in health points.
        /// </summary>
        public float Damage
        {
            get { return damage; }
            set
            {
                Argument.EnsurePositive(value, "Damage");
                if (value > MaxHealth) value = MaxHealth;
                if (value == damage) return;

                damage = value;

                OnDamageChanged();
                if (damage == MaxHealth) OnDied();
            }
        }

        /// <summary>
        /// Gets the maximum amount of health points this <see cref="Unit"/> can have.
        /// </summary>
        public float MaxHealth
        {
            get { return GetStat(UnitStat.MaxHealth); }
        }

        /// <summary>
        /// Gets the amount of health points this <see cref="Unit"/> has.
        /// </summary>
        public float Health
        {
            get { return MaxHealth - damage; }
            set { Damage = MaxHealth - value; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Unit"/> is alive.
        /// </summary>
        /// <remarks>
        /// Dead <see cref="Unit"/>s get garbage collected.
        /// </remarks>
        public bool IsAlive
        {
            get { return Health > 0; }
        }
        #endregion

        /// <summary>
        /// Accesses the <see cref="Task"/> currently executed by this <see cref="Unit"/>.
        /// </summary>
        public Task Task
        {
            get { return task; }
            set
            {
                if (type.IsBuilding && (value is Tasks.Move || value is Tasks.Attack || value is Tasks.Follow || value is Tasks.Harvest))
                    return;
                
                if (task != null) task.OnCancelled(this);
                task = value;
            }
        }
        /// <summary>
        /// get the value indicating if the unit does nothing
        /// </summary>
        public bool IsIdle
        {
            get { return task == null; }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Tests if this <see cref="Unit"/> has a given <see cref="Skill"/>.
        /// </summary>
        /// <typeparam name="TSkill">
        /// The <see cref="Skill"/> this <see cref="Unit"/> should have.
        /// </typeparam>
        /// <returns>True if this <see cref="Unit"/> has that <see cref="Skill"/>, false if not.</returns>
        public bool HasSkill<TSkill>() where TSkill : Skill
        {
            return Type.Skills.Find<TSkill>() != null;
        }

        /// <summary>
        /// Gets the value of a <see cref="UnitStat"/> for this <see cref="Unit"/>.
        /// </summary>
        /// <param name="stat">The <see cref="UnitStat"/> which's value is to be retrieved.</param>
        /// <returns>The value associed with that <see cref="UnitStat"/>.</returns>
        public int GetStat(UnitStat stat)
        {
            return faction.GetStat(type, stat);
        }

        /// <summary>
        /// Tests if a <see cref="Unit"/> is within the line of sight of this <see cref="Unit"/>.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> to be tested.</param>
        /// <returns>
        /// <c>True</c> if it is within the line of sight of this <see cref="Unit"/>, <c>false</c> if not.
        /// </returns>
        public bool CanSee(Unit unit)
        {
            return Circle.SignedDistance(Circle, unit.Circle) <= GetStat(UnitStat.SightRange);
        }

        /// <summary>
        /// Updates this <see cref="Unit"/> for a frame.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last frame, in seconds.</param>
        /// <remarks>
        /// Used by <see cref="UnitRegistry"/>.
        /// </remarks>
        internal void Update(float timeDeltaInSeconds)
        {
            if (task == null)
            {
                Unit unitToAttack = World.Units.InArea(LineOfSight).FirstOrDefault(unit => unit.faction != faction);
                if (unitToAttack != null) Task = new Tasks.Attack(this, unitToAttack);
            }

            if (task != null)
            {
                task.Update(timeDeltaInSeconds);
                if (task.HasEnded) Task = null;
            }
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return "{0} {1}".FormatInvariant(type, faction);
        }
        #endregion
    }
}
