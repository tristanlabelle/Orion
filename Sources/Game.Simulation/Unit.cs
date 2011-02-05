using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;
using System.Collections.ObjectModel;
using Orion.Game.Simulation.Technologies;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Represents an in-game unit, which can be a character, a vehicle or a building,
    /// depending on its <see cref="Unit"/>.
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

        #region UnitType fields
        private string name;
        private string graphicsTemplate;
        private string voicesTemplate;
        private Dictionary<Type, UnitSkill> skills;
        private ReadOnlyCollection<UnitTypeUpgrade> upgrades;
        #endregion

        #region Constructors
        internal Unit(Handle handle, UnitTypeBuilder builder)
            : base(handle)
        {
            Argument.EnsureNotNull(builder, "builder");
            name = builder.Name;
            graphicsTemplate = builder.GraphicsTemplate ?? builder.Name;
            voicesTemplate = builder.VoicesTemplate ?? builder.Name;

            skills = builder.Skills
                .Select(skill => skill.CreateFrozenClone())
                .ToDictionary(skill => skill.GetType());
            skills.Add(typeof(BasicSkill), builder.BasicSkill);
            upgrades = builder.Upgrades.ToList().AsReadOnly();

            Debug.Assert(!HasSkill<AttackSkill>()
                || GetBaseStat(AttackSkill.RangeStat) <= GetBaseStat(BasicSkill.SightRangeStat),
                "{0} has an attack range bigger than its line of sight.".FormatInvariant(name));

            // components stuff
            Spatial spatial = new Spatial(this);
            spatial.CollisionLayer = builder.IsAirborne ? CollisionLayer.Air : CollisionLayer.Ground;
            spatial.SightRange = builder.BasicSkill.SightRange;
            spatial.Size = builder.Size;
            AddComponent(spatial);

            Identity identity = new Identity(this);
            identity.Name = name;
            identity.VisualIdentity = graphicsTemplate;
            identity.SoundIdentity = voicesTemplate;
            identity.LeavesRemains = true;
            identity.IsSelectable = true;
            identity.TemplateEntity = this;
            foreach (UnitTypeUpgrade upgrade in upgrades)
                identity.Upgrades.Add(upgrade);
            identity.TrainType = HasSkill<MoveSkill>()
                ? TrainType.Immaterial
                : TrainType.OnSite;
            identity.AladdiumCost = builder.BasicSkill.AladdiumCost;
            identity.AlageneCost = builder.BasicSkill.AlageneCost;
            AddComponent(identity);
        }

        /// <summary>
        /// Initializes a new <see cref="Unit"/> from its identifier,
        /// <see cref="Unit"/> and <see cref="World"/>.
        /// </summary>
        /// <param name="handle">A unique handle for this <see cref="Unit"/>.</param>
        /// <param name="type">
        /// The <see cref="Unit"/> which determines
        /// the stats and capabilities of this <see cref="Unit"/>
        /// </param>
        /// <param name="position">The initial position of the <see cref="Unit"/>.</param>
        /// <param name="faction">The <see cref="Faction"/> this <see cref="Unit"/> is part of.</param>
        internal Unit(Handle handle, Unit meta, Faction faction, Vector2 position)
            : base(faction.World, handle)
        {
            Argument.EnsureNotNull(meta, "meta");
            Argument.EnsureNotNull(faction, "faction");

            name = meta.Name;
            graphicsTemplate = meta.GraphicsTemplate;
            voicesTemplate = meta.VoicesTemplate;

            skills = meta.skills;
            upgrades = meta.upgrades;

            Debug.Assert(!HasSkill<AttackSkill>()
                || GetBaseStat(AttackSkill.RangeStat) <= GetBaseStat(BasicSkill.SightRangeStat),
                "{0} has an attack range bigger than its line of sight.".FormatInvariant(name));

            // components stuff
            Spatial spatial = (Spatial)meta.GetComponent<Spatial>().Clone(this);
            spatial.Position = position;
            AddComponent(spatial);
            AddComponent(meta.GetComponent<Identity>().Clone(this));

            this.faction = faction;
            this.taskQueue = new TaskQueue(this);
            this.rallyPoint = Center;

            if (IsBuilding)
            {
                Health = 1;
                healthBuilt = 1;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the construction of this <see cref="Unit"/> is completed.
        /// </summary>
        public event Action<Unit> ConstructionCompleted;
        #endregion

        #region Properties
        #region Type-Related
        /// <summary>
        /// Temporary measure until we 
        /// </summary>
        public Unit Type
        {
            get { return this; }
            set
            {
                Argument.EnsureNotNull(value, "Type");

                name = value.Name;
                graphicsTemplate = value.GraphicsTemplate;
                voicesTemplate = value.VoicesTemplate;

                skills = value.skills;
                upgrades = value.upgrades;

                Debug.Assert(!HasSkill<AttackSkill>()
                    || GetBaseStat(AttackSkill.RangeStat) <= GetBaseStat(BasicSkill.SightRangeStat),
                    "{0} has an attack range bigger than its line of sight.".FormatInvariant(name));
            }
        }

        public string Name
        {
            get { return name; }
        }

        public string GraphicsTemplate
        {
            get { return graphicsTemplate; }
        }

        public string VoicesTemplate
        {
            get { return voicesTemplate; }
        }

        public bool IsBuilding
        {
            get { return !HasSkill<MoveSkill>(); }
        }

        public bool IsSuicidable
        {
            get { return false; }
        }

        public ReadOnlyCollection<UnitTypeUpgrade> Upgrades
        {
            get { return upgrades; }
        }
        #endregion

        #region Spatial Component
        public Size Size
        {
            get { return GetComponent<Spatial>().Size; }
        }

        public new Vector2 Position
        {
            get { return GetComponent<Spatial>().Position; }
            set { GetComponent<Spatial>().Position = value; }
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

        public bool KeepsFactionAlive
        {
            get { return HasSkill<TrainSkill>() || HasSkill<AttackSkill>(); }
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
            get { return HasSkill<TrainSkill>() && !BoundingRectangle.ContainsPoint(rallyPoint); }
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
                Debug.Assert(HasSkill<TrainSkill>());
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
            return skills.ContainsKey(typeof(TSkill)); ;
        }

        public bool HasSkill(Type skillType)
        {
            Argument.EnsureNotNull(skillType, "skillType");
            return skills.ContainsKey(skillType);
        }

        public TSkill TryGetSkill<TSkill>() where TSkill : UnitSkill
        {
            UnitSkill skill;
            skills.TryGetValue(typeof(TSkill), out skill);
            return skill as TSkill;
        }

        public int GetBaseStat(UnitStat stat)
        {
            Argument.EnsureNotNull(stat, "stat");

            UnitSkill skill;
            if (!skills.TryGetValue(stat.SkillType, out skill))
            {
                throw new ArgumentException(
                    "Cannot get base stat {0} without skill {1}."
                    .FormatInvariant(stat, stat.SkillName));
            }

            return skill.GetStat(stat);
        }
        /// <summary>
        /// Gets the value of a <see cref="UnitStat"/> for this <see cref="Unit"/>.
        /// </summary>
        /// <param name="stat">The <see cref="UnitStat"/> which's value is to be retrieved.</param>
        /// <returns>The value associed with that <see cref="UnitStat"/>.</returns>
        public int GetStat(UnitStat stat)
        {
            return faction.GetStat(Type, stat);
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
                Debug.Assert(HasComponent<Spatial>(), "Unit has no spatial component!");
                Debug.Assert(other.HasComponent<Spatial>(), "Enemy unit has no spatial component!");
                bool selfIsAirborne = GetComponent<Spatial>().CollisionLayer == CollisionLayer.Air;
                bool otherIsAirborne = GetComponent<Spatial>().CollisionLayer == CollisionLayer.Air;
                if (!selfIsAirborne && otherIsAirborne) return false;
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

        #region Can*** Testing
        public bool CanTransport(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unitType");

            Debug.Assert(unit.HasComponent<Spatial>(), "Unit has no spatial component!");
            bool isGroundUnit = unit.GetComponent<Spatial>().CollisionLayer == CollisionLayer.Ground;
            return HasSkill<TransportSkill>()
                && unit.HasSkill<MoveSkill>()
                && isGroundUnit
                && !unit.HasSkill<TransportSkill>();
        }

        public bool CanBuild(Unit buildingType)
        {
            Argument.EnsureNotNull(buildingType, "buildingType");
            BuildSkill skill = TryGetSkill<BuildSkill>();
            return buildingType.IsBuilding
                && skill != null
                && skill.Supports(buildingType);
        }

        public bool CanTrain(Unit unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            TrainSkill skill = TryGetSkill<TrainSkill>();
            return !unitType.IsBuilding
                && skill != null
                && skill.Supports(unitType);
        }

        public bool CanResearch(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            ResearchSkill skill = TryGetSkill<ResearchSkill>();
            return skill != null
                && skill.Supports(technology);
        }
        #endregion
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

            Debug.Assert(HasComponent<Spatial>(), "Unit has no Spatial component!");
            GetComponent<Spatial>().Angle = (float)Math.Atan2(delta.Y, delta.X);
        }

        protected override Vector2 GetPosition()
        {
            return Position;
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
            UnitStat armorStat = BasicSkill.ArmorStat;
            UnitStat armorTypeStat = BasicSkill.ArmorTypeStat;

            int targetArmor = target.GetStat(armorStat);
            BasicSkill.ArmorTypes targetArmorType = (BasicSkill.ArmorTypes)target.GetStat(BasicSkill.ArmorTypeStat);

            int damage = Math.Max(1, GetStat(AttackSkill.PowerStat) - targetArmor);
            
            if(IsSuperEffectiveAgainst(targetArmorType))
                damage *= 2;
            else if(IsIneffectiveAgainst(targetArmorType))
                damage /= 2;

            target.Health -= damage;
            OnHitting(target, damage);

            int splashRadius = GetStat(AttackSkill.SplashRadiusStat);
            if (splashRadius != 0)
            {
                Circle splashCircle = new Circle(target.Center, splashRadius);
                IEnumerable<Unit> potentiallySplashedUnits = World.Entities
                    .Intersecting(splashCircle)
                    .OfType<Unit>();

                foreach (Unit potentiallySplashedUnit in potentiallySplashedUnits)
                {
                    if (potentiallySplashedUnit == target) continue;
                    if (Faction.GetDiplomaticStance(potentiallySplashedUnit.Faction).HasFlag(DiplomaticStance.AlliedVictory)) continue;

                    float distance = (splashCircle.Center - potentiallySplashedUnit.Center).LengthFast;
                    if (distance > splashRadius) continue;

                    int splashedTargetArmor = potentiallySplashedUnit.GetStat(armorStat);
                    int splashDamage = (int)(Math.Max(1, GetStat(AttackSkill.PowerStat) - targetArmor) * (1 - distance / splashRadius));
                    potentiallySplashedUnit.Health -= splashDamage;
                }
            }

            timeElapsedSinceLastHitInSeconds = 0;
        }

        public bool IsSuperEffectiveAgainst(BasicSkill.ArmorTypes type)
        {
            AttackSkill skill = Type.TryGetSkill<AttackSkill>();
            if (skill == null)
                return false;
            else
                return skill.IsSuperEffectiveAgainst(type);
        }

        public bool IsIneffectiveAgainst(BasicSkill.ArmorTypes type)
        {
            AttackSkill skill = Type.TryGetSkill<AttackSkill>();
            if (skill != null)
                return false;
            else
                return skill.IsIneffectiveAgainst(type);
        }

        private void OnHitting(Unit target, float damage)
        {
            HitEventArgs args = new HitEventArgs(this, target, damage, World.LastSimulationStep.TimeInSeconds);

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
                Debug.Assert(transportedUnit.GetComponent<Spatial>().CollisionLayer == CollisionLayer.Ground);

                Point? location = GridRegion.Points
                    .Concat(GridRegion.GetAdjacentPoints())
                    .FirstOrNull(point => World.IsFree(point, CollisionLayer.Ground));
                if (!location.HasValue)
                {
                    faction.RaiseWarning("Pas de place pour le débarquement d'unités");
                    break;
                }

                transportedUnits.Pop();
                // The position field is changed directly instead of using the property
                // because we don't want to raise the Moved event.
                transportedUnit.Position = location.Value;
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
            SuicideBombSkill suicideBombSkill = TryGetSkill<SuicideBombSkill>();

            Unit explodingTarget = World.Entities
                .Intersecting(Rectangle.FromCenterSize(Center, new Vector2(3, 3)))
                .OfType<Unit>()
                .FirstOrDefault(unit => unit != this
                    && suicideBombSkill.IsExplodingTarget(unit)
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

            bool isGroundUnit = GetComponent<Spatial>().CollisionLayer == CollisionLayer.Ground;
            if (!isGroundUnit && GetStat(AttackSkill.RangeStat) == 0)
                attackableUnits = attackableUnits.Where(u => u.GetComponent<Spatial>().CollisionLayer == CollisionLayer.Ground);

            // HACK: Attack units which can attack first, then other units.
            Unit unitToAttack = attackableUnits
                .WithMinOrDefault(unit => (unit.Position - Position).LengthSquared
                    + (unit.HasSkill<AttackSkill>() ? 0 : 100));

            if (unitToAttack == null) return false;
        
            AttackTask attackTask = new AttackTask(this, unitToAttack);
            taskQueue.Enqueue(attackTask);
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
                   && Faction.GetDiplomaticStance(unit.Faction).HasFlag(DiplomaticStance.AlliedVictory)
                   && IsInLineOfSight(unit))
               .WithMinOrDefault(unit => (unit.Position - Position).LengthSquared);

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
               .WithMinOrDefault(unit => (unit.Position - Position).LengthSquared);

            if (unitToRepair == null) return false;

            RepairTask repairTask = new RepairTask(this, unitToRepair);
            taskQueue.OverrideWith(repairTask);
            return true;
        }
        #endregion

        public override string ToString()
        {
            return "{0} {2} {1}".FormatInvariant(Handle, name, faction);
        }
        #endregion
    }
}

