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
        public event Action<Unit> DamageChanged;

        private void RaiseDamageChanged()
        {
            var handler = DamageChanged;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Raised when the construction of this <see cref="Unit"/> is completed.
        /// </summary>
        public event Action<Unit> ConstructionCompleted;

        private void RaiseConstructionCompleted()
        {
            var handler = ConstructionCompleted;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Raised when this <see cref="Unit"/> hits another <see cref="Unit"/>.
        /// </summary>
        public event Action<Unit, HitEventArgs> Hitting;

        private void RaiseHitting(Unit target, float damage)
        {
            HitEventArgs args = new HitEventArgs(this, target, damage);

            if (Hitting != null) Hitting(this, args);

            World.RaiseUnitHitting(args);
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
            get { return new Circle(Center, GetStat(BasicSkill.SightRangeStat)); }
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
            set
            {
                Debug.Assert(type.IsBuilding);
                rallyPoint = value;
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Skills
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

            if (!HasSkill<AttackSkill>()) return false;

            int attackRange = GetStat(AttackSkill.RangeStat);
            if (attackRange == 0)
            {
                if (!IsAirborne && other.IsAirborne) return false;
                return Region.AreAdjacentOrIntersecting(GridRegion, other.GridRegion);
            }

            return Region.SquaredDistance(GridRegion, other.GridRegion) <= attackRange * attackRange + 0.001f;
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

            RaiseHitting(target, damage);
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
                RaiseConstructionCompleted();
            }
        }

        public void CompleteConstruction()
        {
            Health = MaxHealth;
            isUnderConstruction = false;
            RaiseConstructionCompleted();
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Attempts to find a point surrounding this unit where a unit of a given type could be spawned.
        /// </summary>
        /// <param name="spawneeType">The type of the unit to be spawned.</param>
        /// <returns>A point where such a unit could be spawned, or null if</returns>
        public Point? TryGetFreeSurroundingSpawnPoint(UnitType spawneeType)
        {
            Argument.EnsureNotNull(spawneeType, "spawneeType");

            Region trainerRegion = GridRegion;

            Region spawnRegion = new Region(
                trainerRegion.MinX - spawneeType.Size.Width,
                trainerRegion.MinY - spawneeType.Size.Height,
                trainerRegion.Size.Width + spawneeType.Size.Width,
                trainerRegion.Size.Height + spawneeType.Size.Height);
            var potentialSpawnPoints = spawnRegion.InternalBorderPoints
                .Where(point =>
                    {
                        Region region = new Region(point, spawneeType.Size);
                        return new Region(World.Size).Contains(region)
                            && World.IsFree(new Region(point, spawneeType.Size), spawneeType.CollisionLayer);
                    });

            if (HasRallyPoint)
            {
                return potentialSpawnPoints
                    .Select(point => (Point?)point)
                    .WithMinOrDefault(point => ((Vector2)point - RallyPoint.Value).LengthSquared);
            }
            else
            {
                return potentialSpawnPoints.FirstOrNull();
            }
        }

        /// <summary>
        /// Spawns a unit of a given type around this unit.
        /// </summary>
        /// <param name="spawneeType">The type of the unit to be spawned.</param>
        public Unit TrySpawn(UnitType spawneeType)
        {
            Argument.EnsureNotNull(spawneeType, "spawneeType");

            Point? point = TryGetFreeSurroundingSpawnPoint(spawneeType);
            if (!point.HasValue) return null;

            Unit spawnee = faction.CreateUnit(spawneeType, point.Value);
            Vector2 traineeDelta = spawnee.Center - Center;
            spawnee.Angle = (float)Math.Atan2(traineeDelta.Y, traineeDelta.X);

            if (HasRallyPoint) spawnee.ApplyRallyPoint(rallyPoint.Value);

            return spawnee;
        }

        private void ApplyRallyPoint(Vector2 target)
        {
            // Check to see if we can harvest automatically
            if (HasSkill<HarvestSkill>())
            {
                ResourceNode resourceNode = World.Entities
                    .Intersecting(target)
                    .OfType<ResourceNode>()
                    .FirstOrDefault();

                if (resourceNode != null && resourceNode.IsHarvestableByFaction(faction))
                {
                    HarvestTask harvestTask = new HarvestTask(this, resourceNode);
                    taskQueue.OverrideWith(harvestTask);
                    return;
                }
            }

            // Move instead
            Point targetPoint = (Point)target;
            MoveTask moveToRallyPointTask = new MoveTask(this, targetPoint);
            taskQueue.OverrideWith(moveToRallyPointTask);
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
                if (HasSkill<SuicideBombSkill>() && TryExplodeWithNearbyUnit())
                    return;

                if (HasSkill<BuildSkill>() && TryRepairNearbyUnit()) { }
                else if (HasSkill<HealSkill>() && TryHealNearbyUnit()) { }
                else if (!IsUnderConstruction && HasSkill<AttackSkill>() && !HasSkill<BuildSkill>()
                    && TryAttackNearbyUnit()) { }
            }
            taskQueue.Update(step);
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

        private void Explode()
        {
            float explosionRadius = GetStat(SuicideBombSkill.RadiusStat);
            Circle explosionCircle = new Circle(Center, explosionRadius);

            World.RaiseExplosionOccured(explosionCircle);
            Suicide();

            Unit[] damagedUnits = World.Entities
                .Intersecting(explosionCircle)
                .OfType<Unit>()
                .Where(unit => unit != this && unit.IsAlive)
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

        private bool TryAttackNearbyUnit()
        {
            IEnumerable<Unit> attackableUnits = World.Entities
                .Intersecting(LineOfSight)
                .OfType<Unit>()
                .Where(unit => Faction.GetDiplomaticStance(unit.Faction) == DiplomaticStance.Enemy
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
                   && Faction.GetDiplomaticStance(unit.Faction) == DiplomaticStance.Ally
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

        private new void Die()
        {
            taskQueue.Clear();
            base.Die();
        }

        public override string ToString()
        {
            return "{0} {2} {1}".FormatInvariant(Handle, type, faction);
        }
        #endregion
    }
}

