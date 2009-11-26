using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Geometry;

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
        private readonly Queue<Task> taskQueue = new Queue<Task>();
        private readonly UnitType type;
        private readonly Faction faction;
        private Vector2 position;
        private float angle;
        private float damage;
        private Vector2? rallyPoint;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Unit"/> from its identifier,
        /// <see cref="UnitType"/> and <see cref="World"/>.
        /// </summary>
        /// <param name="handle">A unique handle for this <see cref="Unit"/>.</param>
        /// <param name="type">
        /// The <see cref="UnitType"/> which determines
        /// the stats and capabilities of this <see cref="Unit"/>
        /// </param>
        /// <param name="position">The initial position of the <see cref="Unit"/>.</param>
        /// <param name="faction">The <see cref="Faction"/> this <see cref="Unit"/> is part of.</param>
        internal Unit(Handle handle, UnitType type, Faction faction, Vector2 position)
            : base(faction.World, handle)
        {
            Argument.EnsureNotNull(type, "type");
            Argument.EnsureNotNull(faction, "faction");

            this.type = type;
            this.faction = faction;
            this.position = position;

            if (type.HasSkill<Skills.Train>())
            {
                this.rallyPoint = GetDefaultRallyPoint();
            }
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
        public override Size Size
        {
            get { return type.Size; }
        }

        public override Vector2 Position
        {
            get { return position; }
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
                if (damage == MaxHealth) Die();
            }
        }

        /// <summary>
        /// Gets the maximum amount of health points this <see cref="Unit"/> can have.
        /// </summary>
        public int MaxHealth
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
        #endregion

        #region Task
        /// <summary>
        /// Accesses the <see cref="Task"/> currently executed by this <see cref="Unit"/>.
        /// </summary>
        public Task Task
        {
            get
            {
                if (taskQueue.Count == 0) return null;
                return taskQueue.Peek();
            }
            set
            {
                if (taskQueue.Count == 1)
                {
                    Task oldTask = taskQueue.Dequeue();
                    oldTask.OnCancelled(this);
                }

                Debug.Assert(taskQueue.Count == 0);

                if (value != null) taskQueue.Enqueue(value);
            }
        }

        /// <summary>
        /// Return the whole queue of task
        /// </summary>
        public IEnumerable<Task> TaskQueue
        {
            get { return taskQueue; }
        } 

        /// <summary>
        /// get the value indicating if the unit does nothing
        /// </summary>
        public bool IsIdle
        {
            get { return taskQueue.Count == 0; }
        }
        #endregion

        /// <summary>
        /// Accesses the rally point of this <see cref="Unit"/>,
        /// relative to its position. This is used for buildings.
        /// </summary>
        public Vector2? RallyPoint
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
        /// Changes the position of this <see cref="Unit"/>.
        /// </summary>
        /// <param name="value">A new world</param>
        public void SetPosition(Vector2 value)
        {
            if (value == position) return;
            if (!World.Bounds.ContainsPoint(value))
            {
                throw new ArgumentException(
                    "Cannot set the position to a value outside of world bounds.",
                    "Position");
            }

            Vector2 oldPosition = position;
            position = value;
            OnMoved(oldPosition, position);
        }

        /// <summary>
        /// Gets the diplomatic stance of this <see cref="Unit"/> towards another one.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Unit"/> with regard to which the diplomatic stance is to be retrieved.
        /// </param>
        /// <returns>
        /// The <see cref="DiplomaticStance"/> with regard to that <see cref="Unit"/>.
        /// </returns>
        public DiplomaticStance GetDiplomaticStance(Unit other)
        {
            Argument.EnsureNotNull(other, "other");
            return faction.GetDiplomaticStance(other.faction);
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
            Argument.EnsureNotNull(unit, "unit");
            return LineOfSight.ContainsPoint(unit.position);
        }

        /// <summary>
        /// Enqueues a <see cref="Task"/> to be executed by this <see cref="Unit"/>.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to be enqueued.</param>
        public void EnqueueTask(Task task)
        {
            Argument.EnsureNotNull(task, "task");
            taskQueue.Enqueue(task);
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
            if (IsIdle && HasSkill<Skills.Attack>())
            {
                Unit unitToAttack = World.Entities
                    .InArea(LineOfSight)
                    .OfType<Unit>()
                    .Where(unit => GetDiplomaticStance(unit) == DiplomaticStance.Enemy)
                    .WithMinOrDefault(unit => (unit.Position - position).LengthSquared);

                if (unitToAttack != null)
                    Task = new Tasks.Attack(this, unitToAttack);
            }

            if (taskQueue.Count > 0)
            {
                Task task = taskQueue.Peek();
                task.Update(timeDeltaInSeconds);
                if (task.HasEnded) taskQueue.Dequeue();
            }
        }

        public override string ToString()
        {
            return "{0} {2} {1}".FormatInvariant(Handle, type, faction);
        }

        public void Suicide()
        {
            this.Health = 0;
        }

        private Vector2 GetDefaultRallyPoint()
        {
            Vector2 newRallyPoint = new Vector2();
            if (World.Terrain.IsWalkableAndWithinBounds((Point)position))
            {
                if (World.Terrain.IsWalkableAndWithinBounds(new Point((int)position.X, (int)position.Y - 1)))
                {
                    //Dispatch units South
                    newRallyPoint.Y = position.Y - 1;
                    newRallyPoint.X = position.X;
                }
                else if (World.Terrain.IsWalkableAndWithinBounds(new Point((int)position.X, (int)position.Y + 1)))
                {
                    //Dispatch units North
                    newRallyPoint.Y = position.Y + 1;
                    newRallyPoint.X = position.X;
                
                }
                else if (World.Terrain.IsWalkableAndWithinBounds(new Point((int)position.X + 1, (int)position.Y)))
                {
                    //Dispatch units Est
                    newRallyPoint.Y = position.Y;
                    newRallyPoint.X = position.X + 1;
                }
                else
                {
                    //Dispatch units West
                    newRallyPoint.Y = position.Y;
                    newRallyPoint.X = position.X - 1;
                }
            }
   
            return newRallyPoint;
        }
        #endregion
    }
}

