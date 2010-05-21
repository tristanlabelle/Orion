using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Simulation
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
        /// The number of frames between successive proximity checks.
        /// Used as an optimization.
        /// </summary>
        private const int nearbyEnemyCheckPeriod = 8;

        private readonly Faction faction;
        private readonly TaskQueue taskQueue;
        private UnitType type;
        private Vector2 position;
        private float angle;
        private float damage;
        private Vector2 rallyPoint;
        /// <summary>
        /// The amount of health that has been built.
        /// A value of <see cref="float.NaN"/> indicates that the construction has completed.
        /// </summary>
        private float healthBuilt = float.NaN;
        private float timeElapsedSinceLastHitInSeconds = float.PositiveInfinity;
        private Stack<Unit> transportedUnits;
        private Unit transporter;
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
            this.rallyPoint = Center;

            if (type.IsBuilding)
            {
                Health = 1;
                healthBuilt = 1;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the type of this unit changes.
        /// </summary>
        public event ValueChangedEventHandler<Unit, UnitType> TypeChanged;

        /// <summary>
        /// Raised when this <see cref="Unit"/> gets damaged or healed.
        /// </summary>
        public event Action<Unit> DamageChanged;

        /// <summary>
        /// Raised when the construction of this <see cref="Unit"/> is completed.
        /// </summary>
        public event Action<Unit> ConstructionCompleted;

        /// <summary>
        /// Raised when this <see cref="Unit"/> hits another <see cref="Unit"/>.
        /// </summary>
        public event Action<Unit, HitEventArgs> Hitting;
        #endregion

        #region Properties
        #region Type-Related
        /// <summary>
        /// Gets the <see cref="UnitType"/> of this <see cref="Unit"/>.
        /// </summary>
        public UnitType Type
        {
            get { return type; }
            set
            {
                Argument.EnsureNotNull(value, "Type");
                if (value == type) return;

                if (value.IsAirborne != type.IsAirborne)
                    throw new ArgumentException("A unit type upgrade cannot change airborneness.", "Type");
                if (value.Size != type.Size)
                    throw new ArgumentException("A unit type upgrade cannot change the unit size.", "Type");

                UnitType oldType = type;
                type = value;
                OnTypeChanged(oldType, value);
            }
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

        #region World & Faction
        public override bool IsAliveInWorld
        {
            get { return IsAlive && transporter == null; }
        }

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

        public new Vector2 Position
        {
            get { return position; }
            set
            {
                if (value == position) return;
                if (!World.Bounds.ContainsPoint(value))
                {
                    Debug.Fail("Position out of bounds.");
                    value = World.Bounds.Clamp(value);
                }

                Vector2 oldPosition = position;
                position = value;
                OnMoved(oldPosition, position);
            }
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
            get { return new Circle(Center, GetStat(BasicSkill.SightRangeStat)); }
        }

        #region Transport
        /// <summary>
        /// Gets a value indicating if this unit is embarked in another unit.
        /// </summary>
        public bool IsEmbarked
        {
            get { return transporter != null; }
        }

        /// <summary>
        /// Gets a value indicating if this transporter unit is at full capacity.
        /// </summary>
        public bool IsTransportFull
        {
            get
            {
                Debug.Assert(HasSkill<TransportSkill>());
                return transportedUnits != null && transportedUnits.Count == GetStat(TransportSkill.CapacityStat);
            }
        }
        #endregion

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

                DamageChanged.Raise(this);
                if (damage == MaxHealth) Die();
            }
        }

        /// <summary>
        /// Gets the maximum amount of health points this <see cref="Unit"/> can have.
        /// </summary>
        public int MaxHealth
        {
            get { return GetStat(BasicSkill.MaxHealthStat); }
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
        /// Gets a value indicating if this unit is under construction.
        /// </summary>
        public bool IsUnderConstruction
        {
            get { return !float.IsNaN(healthBuilt); }
        }

        /// <summary>
        /// Gets the progress of the construction as a value from zero to one.
        /// </summary>
        public float ConstructionProgress
        {
            get { return IsUnderConstruction ? Math.Min(healthBuilt / MaxHealth, 1) : 1; }
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

        /// <summary>
        /// Accesses the time elapsed since this <see cref="Unit"/> last hit something, in seconds.
        /// </summary>
        public float TimeElapsedSinceLastHitInSeconds
        {
            get { return timeElapsedSinceLastHitInSeconds; }
        }
        #endregion

        #region Rally Point
        /// <summary>
        /// Gets a value indicating if this <see cref="Unit"/> has a rally point.
        /// </summary>
        public bool HasRallyPoint
        {
            get { return type.HasSkill<TrainSkill>() && !BoundingRectangle.ContainsPoint(rallyPoint); }
        }

        /// <summary>
        /// Accesses the rally point of this <see cref="Unit"/>, in absolute coordinates.
        /// This is used for buildings.
        /// </summary>
        public Vector2 RallyPoint
        {
            get { return rallyPoint; }
            set
            {
                Debug.Assert(type.HasSkill<TrainSkill>());
                rallyPoint = value;
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Skills/Type
        /// <summary>
        /// Tests if this <see cref="Unit"/> has a given <see cref="UnitSkill"/>.
        /// </summary>
        /// <typeparam name="TSkill">The type of skill to be found.</typeparam>
        /// <returns>True if this <see cref="Unit"/> has that <see cref="UnitSkill"/>, false if not.</returns>
        public bool HasSkill<TSkill>() where TSkill : UnitSkill
        {
            return type.HasSkill<TSkill>();
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
        /// <param name="other">The <see cref="Entity"/> to be tested.</param>
        /// <returns>
        /// <c>True</c> if it is within the line of sight of this <see cref="Unit"/>, <c>false</c> if not.
        /// </returns>
        public bool IsInLineOfSight(Entity other)
        {
            Argument.EnsureNotNull(other, "other");
            return Intersection.Test(LineOfSight, other.BoundingRectangle);
        }

        public bool IsWithinAttackRange(Unit other)
        {
            Argument.EnsureNotNull(other, "other");
            Debug.Assert(HasSkill<AttackSkill>());

            int range = GetStat(AttackSkill.RangeStat);
            if (range == 0)
            {
                if (!IsAirborne && other.IsAirborne) return false;
                return Region.AreAdjacentOrIntersecting(GridRegion, other.GridRegion);
            }

            return Region.SquaredDistance(GridRegion, other.GridRegion) <= range * range + 0.001f;
        }

        public bool IsWithinHealingRange(Unit other)
        {
            Argument.EnsureNotNull(other, "other");
            Debug.Assert(HasSkill<HealSkill>());

            int range = GetStat(HealSkill.RangeStat);
            return Region.SquaredDistance(GridRegion, other.GridRegion) <= range * range + 0.001f;
        }

        private void OnTypeChanged(UnitType oldType, UnitType newType)
        {
            taskQueue.Clear();
            if (TypeChanged != null)
                TypeChanged(this, oldType, newType);
            Faction.OnUnitTypeChanged(this, oldType, newType);
        }
        #endregion

        #region Position/Angle
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

        protected override Vector2 GetPosition()
        {
            return Position;
        }

        protected override void OnMoved(Vector2 oldPosition, Vector2 newPosition)
        {
            faction.OnUnitMoved(this, oldPosition, newPosition);
            base.OnMoved(oldPosition, newPosition);
        }
        #endregion

        #region Hitting
        public bool TryHit(Unit target)
        {
            Argument.EnsureNotNull(target, "target");
            float hitDelayInSeconds = GetStat(AttackSkill.DelayStat);
            if (timeElapsedSinceLastHitInSeconds < hitDelayInSeconds)
                return false;
            Hit(target);
            return true;
        }

        public void Hit(Unit target)
        {
            Argument.EnsureNotNull(target, "target");

            bool isMelee = GetStat(AttackSkill.RangeStat) == 0;
            int targetArmor = target.GetStat(isMelee ? BasicSkill.MeleeArmorStat : BasicSkill.RangedArmorStat);
            int damage = Math.Max(1, GetStat(AttackSkill.PowerStat) - targetArmor);

            target.Health -= damage;

            timeElapsedSinceLastHitInSeconds = 0;

            OnHitting(target, damage);
        }

        private void OnHitting(Unit target, float damage)
        {
            HitEventArgs args = new HitEventArgs(this, target, damage);

            if (Hitting != null) Hitting(this, args);

            World.OnUnitHitting(args);
        }
        #endregion

        #region Building
        public void Build(float amount)
        {
            Argument.EnsurePositive(amount, "amount");

            if (!IsUnderConstruction)
            {
                Debug.Fail("Cannot build a building not under construction.");
                return;
            }

            Health += amount;
            healthBuilt += amount;
            if (healthBuilt >= MaxHealth) OnConstructionCompleted();
        }

        public void CompleteConstruction()
        {
            if (!IsUnderConstruction)
            {
                Debug.Fail("Cannot complete the construction of a building not under construction.");
                return;
            }

            Health = MaxHealth;
            OnConstructionCompleted();
        }

        private void OnConstructionCompleted()
        {
            Debug.Assert(IsUnderConstruction);
            healthBuilt = float.NaN;
            ConstructionCompleted.Raise(this);
            faction.OnBuildingConstructionCompleted(this);
        }
        #endregion

        #region Embarking
        /// <summary>
        /// Embarks another unit in this one.
        /// </summary>
        /// <param name="unit">The unit to be embarked.</param>
        internal void Embark(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            Argument.EnsureEqual(unit.Faction, faction, "unit.Faction");
            Debug.Assert(HasSkill<TransportSkill>());

            if (transportedUnits == null) transportedUnits = new Stack<Unit>();

            int transportCapacity = GetStat(TransportSkill.CapacityStat);
            Debug.Assert(transportedUnits.Count < transportCapacity);
            Debug.Assert(!transportedUnits.Contains(unit));
            Debug.Assert(unit != this);

            unit.taskQueue.Clear();
            World.Entities.Remove(unit);
            transportedUnits.Push(unit);
            unit.transporter = this;
        }

        /// <summary>
        /// Disembarks transported units.
        /// </summary>
        public void Disembark()
        {
            Debug.Assert(HasSkill<TransportSkill>());

            if (transportedUnits == null) return;

            while (transportedUnits.Count > 0)
            {
                Unit transportedUnit = transportedUnits.Peek();

                Point? location = GridRegion.Points
                    .Concat(GridRegion.GetAdjacentPoints())
                    .FirstOrNull(point => World.IsFree(point, transportedUnit.CollisionLayer));
                if (!location.HasValue)
                {
                    faction.RaiseWarning("Pas de place pour le débarquement d'unités");
                    break;
                }

                transportedUnits.Pop();
                // The position field is changed directly instead of using the property
                // because we don't want to raise the Moved event.
                transportedUnit.position = location.Value;
                transportedUnit.transporter = null;
                World.Entities.Add(transportedUnit);
            }
        }
        #endregion

        #region Exploding
        private void Explode()
        {
            float explosionRadius = GetStat(SuicideBombSkill.RadiusStat);
            Circle explosionCircle = new Circle(Center, explosionRadius);

            World.OnExplosionOccured(explosionCircle);
            Suicide();

            Unit[] damagedUnits = World.Entities
                .Intersecting(explosionCircle)
                .OfType<Unit>()
                .Where(unit => unit != this && unit.IsAliveInWorld)
                .ToArray();

            float explosionDamage = GetStat(SuicideBombSkill.DamageStat);
            foreach (Unit damagedUnit in damagedUnits)
            {
                if (damagedUnit.HasSkill<SuicideBombSkill>()) continue;
                float distanceFromCenter = (explosionCircle.Center - damagedUnit.Center).LengthFast;
                float damage = (1 - (float)Math.Pow(distanceFromCenter / explosionCircle.Radius, 5))
                    * explosionDamage;
                damagedUnit.Health -= damage;
            }

            foreach (Unit damagedUnit in damagedUnits)
            {
                if (!damagedUnit.HasSkill<SuicideBombSkill>()) continue;
                damagedUnit.Explode();
            }
        }
        #endregion

        #region Dying
        public void Suicide()
        {
            Health = 0;
        }

        protected override void OnDied()
        {
            if (transportedUnits != null)
            {
                while (transportedUnits.Count > 0)
                    transportedUnits.Pop().Suicide();
            }

            taskQueue.Clear();
            base.OnDied();
            Faction.OnUnitDied(this);
        }
        #endregion

        #region Updating
        protected override void DoUpdate(SimulationStep step)
        {
            timeElapsedSinceLastHitInSeconds += step.TimeDeltaInSeconds;

            // OPTIM: As checking for nearby units takes a lot of processor time,
            // we only do it once every few frames. We take our handle value
            // so the units do not make their checks all at once.
            if (CanPerformProximityChecks(step) && IsIdle)
            {
                if (HasSkill<SuicideBombSkill>() && TryExplodeWithNearbyUnit())
                    return;

                if (HasSkill<BuildSkill>() && TryRepairNearbyUnit()) { }
                else if (HasSkill<HealSkill>() && TryHealNearbyUnit()) { }
                else if (!IsUnderConstruction && HasSkill<AttackSkill>() && !HasSkill<BuildSkill>()
                    && TryAttackNearbyUnit()) { }
            }
            taskQueue.Update(step);
        }

        /// <summary>
        /// Tests if a frame is one in which this unit can perform proximity tests.
        /// This is used to limit the number of such operations and distribute them in time.
        /// </summary>
        /// <param name="step">The current simulation step.</param>
        /// <returns>A value indicating if proximity checks can be performed.</returns>
        internal bool CanPerformProximityChecks(SimulationStep step)
        {
            return (step.Number + (int)Handle.Value) % nearbyEnemyCheckPeriod == 0;
        }

        private bool TryExplodeWithNearbyUnit()
        {
            SuicideBombSkill suicideBombSkill = type.TryGetSkill<SuicideBombSkill>();

            Unit explodingTarget = World.Entities
                .Intersecting(Rectangle.FromCenterSize(Center, new Vector2(3, 3)))
                .OfType<Unit>()
                .FirstOrDefault(unit => unit != this
                    && suicideBombSkill.IsExplodingTarget(unit.type)
                    && Region.AreAdjacentOrIntersecting(GridRegion, unit.GridRegion));

            if (explodingTarget == null) return false;

            float explosionRadius = GetStat(SuicideBombSkill.RadiusStat);
            Circle explosionCircle = new Circle((Center + explodingTarget.Center) * 0.5f, explosionRadius);

            explodingTarget.Suicide();
            Explode();

            return true;
        }

        private bool TryAttackNearbyUnit()
        {
            IEnumerable<Unit> attackableUnits = World.Entities
                .Intersecting(LineOfSight)
                .OfType<Unit>()
                .Where(unit => !Faction.GetDiplomaticStance(unit.Faction).HasFlag(DiplomaticStance.AlliedVictory)
                    && IsInLineOfSight(unit));

            if (!IsAirborne && GetStat(AttackSkill.RangeStat) == 0)
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
                   && !Faction.GetDiplomaticStance(unit.Faction).HasFlag(DiplomaticStance.AlliedVictory)
                   && IsInLineOfSight(unit))
               .WithMinOrDefault(unit => (unit.Position - position).LengthSquared);

            if (unitToHeal == null) return false;

            HealTask healTask = new HealTask(this, unitToHeal);
            taskQueue.OverrideWith(healTask);
            return true;
        }

        private bool TryRepairNearbyUnit()
        {
            Unit unitToRepair = World.Entities
               .Intersecting(LineOfSight)
               .OfType<Unit>()
               .Where(unit => unit != this
                   && unit.IsBuilding
                   && unit.IsUnderConstruction
                   && unit.Faction == faction
                   && IsInLineOfSight(unit))
               .WithMinOrDefault(unit => (unit.Position - position).LengthSquared);

            if (unitToRepair == null) return false;

            RepairTask repairTask = new RepairTask(this, unitToRepair);
            taskQueue.OverrideWith(repairTask);
            return true;
        }
        #endregion

        public override string ToString()
        {
            return "{0} {2} {1}".FormatInvariant(Handle, type, faction);
        }
        #endregion
    }
}

