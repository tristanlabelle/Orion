using System;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Geometry;
using System.Collections.Generic; 

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents an in-game unit, which can be a character, a vehicle or a building,
    /// depending on its <see cref="UnitType"/>.
    /// </summary>
    [Serializable]
    public sealed class Unit : Entity
    {
        #region Fields
        private readonly UnitType type;
        private readonly Faction faction;
        private Vector2 position;
        private float angle;
        private float damage;
        private Task task = null;
        private Vector2 rallyPoint; 
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
        /// <param name="position">The initial position of the <see cref="Unit"/>.</param>
        /// <param name="faction">The <see cref="Faction"/> this <see cref="Unit"/> is part of.</param>
        internal Unit(int id, UnitType type, Faction faction, Vector2 position)
            : base(faction.World, id)
        {
            Argument.EnsureNotNull(type, "type");
            Argument.EnsureNotNull(faction, "faction");

            this.type = type;
            this.faction = faction;
            this.position = position;
            this.rallyPoint = SetDefaultRallyPoint(position);
        }
        #endregion

        #region Events
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
        #endregion

        #region Properties
        #region Identification
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
        /// Accesses the <see cref="Faction"/> which this <see cref="Unit"/> is a member of.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
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

                Rectangle oldBoundingRectangle = BoundingRectangle;
                position = value;
                OnBoundingRectangleChanged(oldBoundingRectangle, BoundingRectangle);
            }
        }

        public override Rectangle BoundingRectangle
        {
            get { return Rectangle.FromCenterSize(position.X, position.Y, type.WidthInTiles, type.HeightInTiles); }
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

        #region Task
        /// <summary>
        /// Accesses the <see cref="Task"/> currently executed by this <see cref="Unit"/>.
        /// </summary>
        public Task Task
        {
            get { return task; }
            set
            {
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

        /// <summary>
        /// Accesses the rally point of this <see cref="Unit"/>,
        /// relative to its position. This is used for buildings.
        /// </summary>
        public Vector2 RallyPoint
        {
            get { return rallyPoint; }
            set { rallyPoint = value; }
        }
        #endregion
        #endregion

        #region Methods
        #region Skills
        /// <summary>
        /// Tests if this <see cref="Unit"/> has a given <see cref="Skill"/>.
        /// </summary>
        /// <typeparam name="TSkill">
        /// The <see cref="Skill"/> this <see cref="Unit"/> should have.
        /// </typeparam>
        /// <returns>True if this <see cref="Unit"/> has that <see cref="Skill"/>, false if not.</returns>
        public TSkill GetSkill<TSkill>() where TSkill : Skill
        {
            return Type.GetSkill<TSkill>();
        }

        /// <summary>
        /// Tests if this <see cref="Unit"/> has a given <see cref="Skill"/>.
        /// </summary>
        /// <typeparam name="TSkill">
        /// The <see cref="Skill"/> this <see cref="Unit"/> should have.
        /// </typeparam>
        /// <returns>True if this <see cref="Unit"/> has that <see cref="Skill"/>, false if not.</returns>
        public bool HasSkill<TSkill>() where TSkill : Skill
        {
            return Type.HasSkill<TSkill>();
        }
        #endregion

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
            Argument.EnsureNotNull(unit, "unit");
            return LineOfSight.ContainsPoint(unit.position);
        }

        /// <summary>
        /// Updates this <see cref="Unit"/> for a frame.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last frame, in seconds.</param>
        /// <remarks>
        /// Used by <see cref="UnitRegistry"/>.
        /// </remarks>
        internal override void Update(float timeDeltaInSeconds)
        {
            if (task == null && HasSkill<Skills.Attack>())
            {
                Unit unitToAttack = World.Entities
                    .InArea(LineOfSight)
                    .OfType<Unit>()
                    .FirstOrDefault(unit => unit.Faction != faction);

                if (unitToAttack != null)
                    Task = new Tasks.Attack(this, unitToAttack);
            }

            if (task != null)
            {
                task.Update(timeDeltaInSeconds);
                if (task.HasEnded) Task = null;
            }
        }

        public override string ToString()
        {
            return "#{0} {2} {1}".FormatInvariant(ID, type, faction);
        }

        public void Kill()
        {
            this.Health = 0;
        }
        private Vector2 SetDefaultRallyPoint(Vector2 startingPosition)
        {
            Vector2 newRallyPoint = new Vector2();
            if (World.Terrain.IsWalkableAndWithinBounds((int)startingPosition.X, (int)startingPosition.Y))
            {
                if (World.Terrain.IsWalkableAndWithinBounds((int)startingPosition.X, (int)startingPosition.Y - 1))
                {
                    //Dispatch units South
                    newRallyPoint.Y = startingPosition.Y - 1;
                    newRallyPoint.X = startingPosition.X;
                }
                else if (World.Terrain.IsWalkableAndWithinBounds((int)startingPosition.X, (int)startingPosition.Y + 1))
                {
                    //Dispatch units North
                    newRallyPoint.Y = startingPosition.Y + 1;
                    newRallyPoint.X = startingPosition.X;
                
                }
                else if (World.Terrain.IsWalkableAndWithinBounds((int)startingPosition.X + 1, (int)startingPosition.Y))
                {
                    //Dispatch units Est
                    newRallyPoint.Y = startingPosition.Y;
                    newRallyPoint.X = startingPosition.X + 1;
                }
                else
                {
                    //Dispatch units West
                    newRallyPoint.Y = startingPosition.Y;
                    newRallyPoint.X = startingPosition.X - 1;
                }
            }
   
            return newRallyPoint;
        }
        #endregion
    }
}

