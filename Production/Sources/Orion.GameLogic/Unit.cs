using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Geometry;
using Orion.GameLogic.Tasks;
using Orion.GameLogic.Skills;

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
        /// <summary>
        /// The number of frames between successive checks for nearby enemies.
        /// Used as an optimization.
        /// </summary>
        private const int nearbyEnemyCheckPeriod = 8;

        private readonly UnitType type;
        private readonly Faction faction;
        private readonly TaskQueue taskQueue;
        private Vector2 position;
        private float angle;
        private float damage;
        private Vector2? rallyPoint;
        private float healthBuilt;
        private bool isUnderConstruction;
        private float timeElapsedSinceLastHitInSeconds = float.PositiveInfinity;
        private Stack<Unit> transportedUnits = new Stack<Unit>();
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
            this.taskQueue = new TaskQueue(this);
            this.position = position;

            if (type.IsBuilding)
            {
                Health = 1;
                healthBuilt = 1;
                isUnderConstruction = true;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Unit"/> gets damaged or healed.
        /// </summary>
        public event GenericEventHandler<Unit> DamageChanged;

        private void RaiseDamageChanged()
        {
            var handler = DamageChanged;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Raised when the construction of this <see cref="Unit"/> is completed.
        /// </summary>
        public event GenericEventHandler<Unit> ConstructionComplete;

        private void RaiseConstructionComplete()
        {
            var handler = ConstructionComplete;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Raised when this <see cref="Unit"/> has hit another <see cref="Unit"/>.
        /// </summary>
        public event GenericEventHandler<Unit, HitEventArgs> Hitted;

        private void RaiseHitted(Unit target, float damage)
        {
            HitEventArgs args = new HitEventArgs(this, target, damage);

            if (Hitted != null) Hitted(this, args);

            World.RaiseUnitHit(args);
        }
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

        public override CollisionLayer CollisionLayer
        {
            get { return type.CollisionLayer; }
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
                if (float.IsNaN(value)) throw new ArgumentException("The damage cannot be set to NaN.", "Damage");
                if (value < 0) value = 0;
                else if (value > MaxHealth) value = MaxHealth;
                else if (value == damage) return;

                damage = value;

                RaiseDamageChanged();
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
            get { return isUnderConstruction ? Math.Min(healthBuilt / MaxHealth, 1) : 1; }
        }
        #endregion

        #region Tasks
        /// <summary>
        /// Gets the queue of this unit's tasks.
        /// </summary>
        public TaskQueue TaskQueue
        {
            get { return taskQueue; }
        }

        /// <summary>
        /// Gets a value indicating if the unit does nothing.
        /// </summary>
        public bool IsIdle
        {
            get { return taskQueue.Count == 0; }
        }
        #endregion

        /// <summary>
        /// Accesses the time elapsed since this <see cref="Unit"/> last hit something, in seconds.
        /// </summary>
        public float TimeElapsedSinceLastHitInSeconds
        {
            get { return timeElapsedSinceLastHitInSeconds; }
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

        public bool IsTransportFull
        {
            get { return transportedUnits.Count == GetStat(UnitStat.TransportCapacity); }
        }
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
        /// <param name="other">The <see cref="Entity"/> to be tested.</param>
        /// <returns>
        /// <c>True</c> if it is within the line of sight of this <see cref="Unit"/>, <c>false</c> if not.
        /// </returns>
        public bool IsInLineOfSight(Entity other)
        {
            Argument.EnsureNotNull(other, "other");
            return Intersection.Test(LineOfSight, other.BoundingRectangle);
        }

        public bool HasWithinAttackRange(Unit other)
        {
            Argument.EnsureNotNull(other, "other");
            if (!HasSkill<AttackSkill>()) return false;
            int attackRange = GetStat(UnitStat.AttackRange);
            if (attackRange == 0)
            {
                if (!IsAirborne && other.IsAirborne)
                    return false;
                return Region.AreAdjacentOrIntersecting(GridRegion, other.GridRegion);
            }
            return (Center - other.Center).LengthSquared <= attackRange * attackRange;
        }
        #endregion

        #region Position/Angle
        /// <summary>
        /// Changes the position of this <see cref="Unit"/>.
        /// </summary>
        /// <param name="value">A new world</param>
        public void SetPosition(Vector2 value)
        {
            if (value == position) return;
            if (!World.Bounds.ContainsPoint(value))
            {
                Debug.Fail("Position out of bounds.");
                value = World.Bounds.Clamp(value);
            }

            Vector2 oldPosition = position;
            position = value;
            RaiseMoved(oldPosition, position);
        }

        /// <summary>
        /// Rotates this <see cref="Unit"/> so that it faces a target.
        /// </summary>
        /// <param name="target">The location of the target to be faced.</param>
        public void LookAt(Vector2 target)
        {
            Vector2 delta = target - Center;
            if (delta.LengthSquared < 0.01f) return;
            Angle = (float)Math.Atan2(delta.Y, delta.X);
        }
        #endregion

        #region Actions
        public bool TryHit(Unit target)
        {
            Argument.EnsureNotNull(target, "target");
            float hitDelayInSeconds = GetStat(UnitStat.AttackDelay);
            if (timeElapsedSinceLastHitInSeconds < hitDelayInSeconds)
                return false;
            Hit(target);
            return true;
        }

        public void Hit(Unit target)
        {
            Argument.EnsureNotNull(target, "target");

            bool isMelee = GetStat(UnitStat.AttackRange) == 0;
            int targetArmor = target.GetStat(isMelee ? UnitStat.MeleeArmor : UnitStat.RangedArmor);
            int damage = Math.Max(1, GetStat(UnitStat.AttackPower) - targetArmor);

            target.Health -= damage;

            timeElapsedSinceLastHitInSeconds = 0;

            RaiseHitted(target, damage);
        }

        public void Suicide()
        {
            Health = 0;
        }

        public void Build(float amount)
        {
            Argument.EnsurePositive(amount, "amount");
            Health += amount;
            healthBuilt += amount;
            if (healthBuilt >= MaxHealth)
            {
                isUnderConstruction = false;
                RaiseConstructionComplete();
            }
        }

        public void CompleteConstruction()
        {
            Health = MaxHealth;
            isUnderConstruction = false;
            RaiseConstructionComplete();
        }

        public void Embark(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            Argument.EnsureEqual(unit.Faction, faction, "unit.Faction");
            Debug.Assert(HasSkill<TransportSkill>());
            Debug.Assert(!IsTransportFull);
            Debug.Assert(unit != this);
            Debug.Assert(!transportedUnits.Contains(unit));

            transportedUnits.Push(unit);

            unit.taskQueue.Clear();
            World.Entities.Remove(unit);
        }

        public void Disembark()
        {
            while (transportedUnits.Count > 0)
            {
                Unit transportedUnit = transportedUnits.Peek();

                Point? location = GridRegion.GetAdjacentPoints()
                    .FirstOrNull(point => World.IsFree(point, transportedUnit.CollisionLayer));
                if (!location.HasValue)
                {
                    faction.RaiseWarning("Pas de place pour le débarquement d'unités");
                    break;
                }

                transportedUnits.Pop();
                // HACK: Change the position directly instead of using SetPosition
                // because we don't want to raise the Moved event, EntityManager
                // doesn't like that.
                transportedUnit.position = location.Value;
                World.Entities.Add(transportedUnit);
            }
        }
        #endregion

        protected override void DoUpdate(SimulationStep step)
        {
            timeElapsedSinceLastHitInSeconds += step.TimeDeltaInSeconds;

            // OPTIM: As checking for nearby units takes a lot of processor time,
            // we only do it once every few frames. We take our handle value
            // so the units do not make their checks all at once.
            if ((step.Number + (int)Handle.Value % nearbyEnemyCheckPeriod)
                % nearbyEnemyCheckPeriod == 0 && IsIdle)
            {
                if (!IsUnderConstruction && HasSkill<AttackSkill>() && !HasSkill<BuildSkill>())
                    TryAttackNearbyUnit();
                else if (HasSkill<HealSkill>())
                    TryHealNearbyUnit();
            }
            taskQueue.Update(step);
        }

        private bool TryAttackNearbyUnit()
        {
            IEnumerable<Unit> attackableUnits = World.Entities
                .Intersecting(LineOfSight)
                .OfType<Unit>()
                .Where(unit => Faction.GetDiplomaticStance(unit.Faction) == DiplomaticStance.Enemy
                    && IsInLineOfSight(unit));

            if (!IsAirborne && GetStat(UnitStat.AttackRange) == 0)
                attackableUnits = attackableUnits.Where(u => !u.IsAirborne);

            // HACK: Attack units which can attack first, then other units.
            Unit unitToAttack = attackableUnits
                .WithMinOrDefault(unit => (unit.Position - position).LengthSquared
                    + (unit.HasSkill<AttackSkill>() ? 0 : 100));

            if (unitToAttack == null) return false;
        
            AttackTask attackTask = new AttackTask(this, unitToAttack);
            taskQueue.OverrideWith(attackTask);
            return true;
        }

        private bool TryHealNearbyUnit()
        {
            Unit unitToHeal = World.Entities
               .Intersecting(LineOfSight)
               .OfType<Unit>()
               .Where(unit => unit != this
                   && unit.Damage > 0
                   && !unit.IsBuilding
                   && Faction.GetDiplomaticStance(unit.Faction) == DiplomaticStance.Ally
                   && IsInLineOfSight(unit))
               .WithMinOrDefault(unit => (unit.Position - position).LengthSquared);

            if (unitToHeal == null) return false;

            HealTask healTask = new HealTask(this, unitToHeal);
            taskQueue.OverrideWith(healTask);
            return true;
        }

        public override string ToString()
        {
            return "{0} {2} {1}".FormatInvariant(Handle, type, faction);
        }
        #endregion
    }
}

