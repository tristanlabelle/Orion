﻿using System;
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
        private readonly UnitType type;
        private readonly Faction faction;
        private readonly Queue<Task> queuedTasks = new Queue<Task>();
        private Vector2 position;
        private float angle;
        private float damage;
        private Vector2? rallyPoint;
        private float healthBuilt;
        private bool isUnderConstruction;
        private float timeSinceLastHitInSeconds = 0;
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
            
            if (type.IsBuilding) isUnderConstruction = true;
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
        #region Type-Related
        /// <summary>
        /// Gets the <see cref="UnitType"/> of this <see cref="Unit"/>.
        /// </summary>
        public UnitType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Unit"/> is a building.
        /// </summary>
        public bool IsBuilding
        {
            get { return type.IsBuilding; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Unit"/> moves in the air.
        /// </summary>
        public bool IsAirborne
        {
            get { return type.IsAirborne; }
        }

        public override bool IsSolid
        {
            get { return !type.IsAirborne; }
        }
        #endregion

        #region Faction
        /// <summary>
        /// Accesses the <see cref="Faction"/> which this <see cref="Unit"/> is a member of.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
        }
        #endregion

        #region Physical
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
        #endregion

        /// <summary>
        /// Gets a circle representing the area of the world that is within
        /// the line of sight of this <see cref="Unit"/>.
        /// </summary>
        public Circle LineOfSight
        {
            get { return new Circle(Center, GetStat(UnitStat.SightRange)); }
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
                if (value < 0) value = 0;
                else if (value > MaxHealth) value = MaxHealth;
                else if (value == damage) return;

                damage = value;

                OnDamageChanged();
                if (damage == MaxHealth) Die();
                else if (damage == 0 && IsUnderConstruction)
                    isUnderConstruction = false;
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

        public bool IsUnderConstruction
        {
            get { return isUnderConstruction; }
        }

        /// <summary>
        /// Gets the progress of the construction as a value from zero to one.
        /// </summary>
        public float ConstructionProgress
        {
            get { return isUnderConstruction ? healthBuilt / MaxHealth : 1; }
        }
        #endregion

        #region Tasks
        /// <summary>
        /// Gets the sequence of this unit's queued tasks, in the order they're to be executed.
        /// </summary>
        public IEnumerable<Task> QueuedTasks
        {
            get { return queuedTasks; }
        }

        /// <summary>
        /// Gets a value indicating if this unit's task queue is full.
        /// </summary>
        public bool IsTaskQueueFull
        {
            get { return queuedTasks.Count >= 8; }
        }

        /// <summary>
        /// Accesses the <see cref="Task"/> currently executed by this <see cref="Unit"/>.
        /// </summary>
        public Task CurrentTask
        {
            get
            {
                if (queuedTasks.Count == 0) return null;
                return queuedTasks.Peek();
            }
            set
            {
                if (queuedTasks.Count == 1)
                    queuedTasks.Dequeue();

                Debug.Assert(queuedTasks.Count == 0);

                if (value != null)
                {
                    if (value.Unit != this) throw new ArgumentException("Cannot execute another unit's task.", "CurrentTask");
                    queuedTasks.Enqueue(value);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the unit does nothing.
        /// </summary>
        public bool IsIdle
        {
            get { return queuedTasks.Count == 0; }
        }
        #endregion

        public float TimeSinceLastHitInSeconds
        {
            get { return timeSinceLastHitInSeconds; }
            internal set
            {
                Argument.EnsureNotNull(value, "timeSinceLastHitInSeconds");
                timeSinceLastHitInSeconds = value;
            }
        }

        #region Rally Point
        /// <summary>
        /// Gets a value indicating if this <see cref="Unit"/> has a rally point.
        /// </summary>
        public bool HasRallyPoint
        {
            get { return rallyPoint.HasValue; }
        }

        /// <summary>
        /// Accesses the rally point of this <see cref="Unit"/>, in absolute coordinates.
        /// This is used for buildings.
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

        #region Stats
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
        /// <param name="other">The <see cref="Unit"/> to be tested.</param>
        /// <returns>
        /// <c>True</c> if it is within the line of sight of this <see cref="Unit"/>, <c>false</c> if not.
        /// </returns>
        public bool CanSee(Unit other)
        {
            Argument.EnsureNotNull(other, "other");
            return LineOfSight.ContainsPoint(other.Center);
        }

        public bool IsInAttackRange(Unit other)
        {
            Argument.EnsureNotNull(other, "other");
            if (!HasSkill<Skills.Attack>()) return false;
            int attackRange = GetStat(UnitStat.AttackRange);
            if (attackRange == 0) return Region.AreAdjacentOrIntersecting(GridRegion, other.GridRegion);
            return (Center - other.Center).LengthSquared <= attackRange * attackRange;
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
        /// Enqueues a <see cref="Task"/> to be executed by this <see cref="Unit"/>.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to be enqueued.</param>
        public void EnqueueTask(Task task)
        {
            Argument.EnsureNotNull(task, "task");
            if (task.Unit != this) throw new ArgumentException("Cannot execute another unit's task.", "task");
            queuedTasks.Enqueue(task);
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
                    CurrentTask = new Tasks.Attack(this, unitToAttack);
            }

            if (queuedTasks.Count > 0)
            {
                Task task = queuedTasks.Peek();
                task.Update(timeDeltaInSeconds);
                if (task.HasEnded && queuedTasks.Count > 0 && queuedTasks.Peek() == task)
                    queuedTasks.Dequeue();
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

        public void Build(float health)
        {
            Argument.EnsurePositive(health, "health");
            this.Health += health;
            this.healthBuilt += health;
            if (healthBuilt >= MaxHealth)
                isUnderConstruction = false;
        }

        public void CompleteConstruction()
        {
            this.Health = MaxHealth;
            this.isUnderConstruction = false;
        }
        #endregion
    }
}

