using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Technologies;

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

        /// <summary>
        /// The amount of health that has been built.
        /// A value of <see cref="float.NaN"/> indicates that the construction has completed.
        /// </summary>
        private float healthBuilt = float.NaN;

        private Dictionary<Type, UnitSkill> skills;
        #endregion

        #region Constructors
        internal Unit(Handle handle, UnitTypeBuilder type)
            : base(handle)
        {
            Argument.EnsureNotNull(type, "type");

            skills = type.Skills
                .Select(skill => skill.CreateFrozenClone())
                .ToDictionary(skill => skill.GetType());
            skills.Add(typeof(BasicSkill), type.BasicSkill);

            Debug.Assert(!HasSkill<AttackSkill>()
                || GetBaseStat(AttackSkill.RangeStat) <= GetBaseStat(BasicSkill.SightRangeStat),
                "{0} has an attack range bigger than its line of sight.".FormatInvariant(type.Name));

            // components stuff
            Spatial spatial = new Spatial(this);
            spatial.CollisionLayer = type.IsAirborne ? CollisionLayer.Air : CollisionLayer.Ground;
            spatial.SightRange = type.BasicSkill.SightRange;
            spatial.Size = type.Size;
            Components.Add(spatial);

            Identity identity = new Identity(this);
            identity.Name = type.Name;
            identity.VisualIdentity = type.GraphicsTemplate ?? type.Name;
            identity.SoundIdentity = type.VoicesTemplate ?? type.Name;
            identity.LeavesRemains = true;
            identity.IsSelectable = true;
            identity.TemplateEntity = this;
            foreach (UnitTypeUpgrade upgrade in type.Upgrades)
                identity.Upgrades.Add(upgrade);
            identity.TrainType = InternalHasSkill<MoveSkill>() ? TrainType.Immaterial : TrainType.OnSite;
            identity.AladdiumCost = type.BasicSkill.AladdiumCost;
            identity.AlageneCost = type.BasicSkill.AlageneCost;
            Components.Add(identity);

            Health health = new Health(this);
            health.Armor = type.BasicSkill.Armor;
            health.ArmorType = type.BasicSkill.ArmorType;
            health.MaximumValue = type.BasicSkill.MaxHealth;
            Components.Add(health);

            if (InternalHasSkill<MoveSkill>())
            {
                MoveSkill moveSkill = InternalTryGetSkill<MoveSkill>();
                Move move = new Move(this);
                move.Speed = moveSkill.Speed;
                Components.Add(move);
            }

            if (InternalHasSkill<AttackSkill>())
            {
                AttackSkill attackSkill = InternalTryGetSkill<AttackSkill>();
                Attacker attacker = new Attacker(this);
                attacker.Delay = attackSkill.Delay;
                attacker.Power = attackSkill.Power;
                attacker.Range = attackSkill.Range;
                attacker.SplashRadius = attackSkill.SplashRadius;
                Components.Add(attacker);
            }

            if (InternalHasSkill<HarvestSkill>())
            {
                HarvestSkill harvestSkill = InternalTryGetSkill<HarvestSkill>();
                Harvester harvester = new Harvester(this);
                harvester.Speed = harvestSkill.Speed;
                harvester.MaxCarryingAmount = harvestSkill.MaxCarryingAmount;
                Components.Add(harvester);
            }

            if (InternalHasSkill<BuildSkill>())
            {
                BuildSkill buildSkill = InternalTryGetSkill<BuildSkill>();
                Builder builder = new Builder(this);
                builder.Speed = buildSkill.Speed;
                foreach (string target in buildSkill.Targets)
                    builder.BuildableTypes.Add(target);
                Components.Add(builder);
            }

            if (InternalHasSkill<TrainSkill>())
            {
                TrainSkill trainSkill = InternalTryGetSkill<TrainSkill>();
                Trainer trainer = new Trainer(this);
                trainer.SpeedMultiplier = trainSkill.Speed;
                foreach (string target in trainSkill.Targets)
                    trainer.TrainableTypes.Add(target);
                Components.Add(trainer);
            }

            FactionMembership membership = new FactionMembership(this);
            membership.FoodRequirement = type.BasicSkill.FoodCost;
            if (InternalHasSkill<ProvideFoodSkill>())
                membership.FoodProvided = GetBaseStat(ProvideFoodSkill.AmountStat);
            Components.Add(membership);

            Components.Add(new TaskQueue(this));
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
        internal Unit(Handle handle, Unit prototype, Faction faction, Vector2 position)
            : base(faction.World, handle)
        {
            Argument.EnsureNotNull(prototype, "meta");
            Argument.EnsureNotNull(faction, "faction");

            skills = prototype.skills;

            Debug.Assert(!InternalHasSkill<AttackSkill>()
                || GetBaseStat(AttackSkill.RangeStat) <= GetBaseStat(BasicSkill.SightRangeStat),
                "{0} has an attack range bigger than its line of sight.".FormatInvariant(prototype.Name));

            // components stuff
            foreach (Component component in prototype.Components)
                Components.Add(component.Clone(this));

            Components.Get<Spatial>().Position = position;
            Components.Get<FactionMembership>().Faction = faction;
            Trainer trainer = Components.TryGet<Trainer>();
            if (trainer != null) trainer.RallyPoint = Center;

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

                Name = value.Name;
                GraphicsTemplate = value.GraphicsTemplate;
                VoicesTemplate = value.VoicesTemplate;

                skills = value.skills;
                Upgrades = value.Upgrades;

                Debug.Assert(!HasComponent<Attacker, AttackSkill>()
                    || GetBaseStat(AttackSkill.RangeStat) <= GetBaseStat(BasicSkill.SightRangeStat),
                    "{0} has an attack range bigger than its line of sight.".FormatInvariant(Name));
            }
        }

        public string Name
        {
            get { return Components.Get<Identity>().Name; }
            private set { Components.Get<Identity>().Name = value; }
        }

        public string GraphicsTemplate
        {
            get { return Components.Get<Identity>().VisualIdentity; }
            set { Components.Get<Identity>().VisualIdentity = value; }
        }

        public string VoicesTemplate
        {
            get { return Components.Get<Identity>().SoundIdentity; }
            set { Components.Get<Identity>().SoundIdentity = value; }
        }

        public bool IsBuilding
        {
            get { return !HasComponent<Move, MoveSkill>(); }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Unit"/> can commit suicide.
        /// </summary>
        public bool CanSuicide
        {
            get { return true; }
        }

        public ICollection<UnitTypeUpgrade> Upgrades
        {
            get { return Components.Get<Identity>().Upgrades; }
            private set
            {
                Identity identity = Components.Get<Identity>();
                identity.Upgrades.Clear();
                foreach (UnitTypeUpgrade upgrade in value)
                    identity.Upgrades.Add(upgrade);
            }
        }
        #endregion

        #region Spatial Component
        public override Size Size
        {
            get { return Components.Get<Spatial>().Size; }
        }

        public new Vector2 Position
        {
            get { return Components.Get<Spatial>().Position; }
            set { Components.Get<Spatial>().Position = value; }
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
            get { return HasComponent<Trainer, TrainSkill>() || HasComponent<Attacker, AttackSkill>(); }
        }
        #endregion

        #region World & Faction
        public override bool IsAliveInWorld
        {
            get { return IsAlive; }
        }

        /// <summary>
        /// Accesses the <see cref="Faction"/> which this <see cref="Unit"/> is a member of.
        /// </summary>
        public Faction Faction
        {
            get { return Components.Get<FactionMembership>().Faction; }
        }
        #endregion

        #region Physical

        #region Health
        /// <summary>
        /// Accesses the damage that has been inflicted to this <see cref="Unit"/>, in health points.
        /// </summary>
        public float Damage
        {
            get { return Components.Get<Health>().Damage; }
            set { Components.Get<Health>().Damage = value; }
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
            get { return MaxHealth - Damage; }
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
            get { return Components.Get<TaskQueue>(); }
        }

        /// <summary>
        /// Gets a value indicating if the unit does nothing.
        /// </summary>
        public bool IsIdle
        {
            get { return Components.Get<TaskQueue>().IsEmpty; }
        }

        /// <summary>
        /// Accesses the time elapsed since this <see cref="Unit"/> last hit something, in seconds.
        /// </summary>
        public float TimeElapsedSinceLastHitInSeconds
        {
            get { return Components.Get<Attacker>().TimeElapsedSinceLastHit; }
        }
        #endregion

        #region Rally Point
        /// <summary>
        /// Gets a value indicating if this <see cref="Unit"/> has a rally point.
        /// </summary>
        public bool HasRallyPoint
        {
            get { return HasComponent<Trainer, TrainSkill>() && !BoundingRectangle.ContainsPoint(RallyPoint); }
        }

        /// <summary>
        /// Accesses the rally point of this <see cref="Unit"/>, in absolute coordinates.
        /// This is used for buildings.
        /// </summary>
        public Vector2 RallyPoint
        {
            get { return Components.Get<Trainer>().RallyPoint; }
            set { Components.Get<Trainer>().RallyPoint = value; }
        }
        #endregion
        #endregion

        #region Methods
        #region Skills/Type
        /// <summary>
        /// Tests if this <see cref="Unit"/> has a given <see cref="Component"/>,
        /// also checking if it has the corresponding <see cref="UnitSkill"/> to aid migration.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component to be found.</typeparam>
        /// <typeparam name="TSkill">The type of the corresponding skill.</typeparam>
        /// <returns>A value indicating if this <see cref="Unit"/> has a given <see cref="Component"/>.</returns>
        public bool HasComponent<TComponent, TSkill>()
            where TComponent : Component
            where TSkill : UnitSkill
        {
            bool hasComponent = Components.Has<TComponent>();
            bool hasSkill = skills.ContainsKey(typeof(TSkill));

            if (hasComponent != hasSkill)
            {
                string message = "Unit has component {0} without skill {1} or vice-versa."
                    .FormatInvariant(typeof(TComponent).FullName, typeof(TSkill).FullName);
                Debug.Fail(message);
            }

            return hasComponent;
        }

        /// <remarks>
        /// Same as <see cref="HasSkill"/>, but not obsoleted so internal usages do not cause warnings.
        /// </remarks>
        private bool InternalHasSkill<TSkill>() where TSkill : UnitSkill
        {
            return skills.ContainsKey(typeof(TSkill));
        }

        /// <remarks>
        /// Same as <see cref="TryGetSkill"/>, but not obsoleted so internal usages do not cause warnings.
        /// </remarks>
        public TSkill InternalTryGetSkill<TSkill>() where TSkill : UnitSkill
        {
            UnitSkill skill;
            skills.TryGetValue(typeof(TSkill), out skill);
            return skill as TSkill;
        }

        /// <summary>
        /// Tests if this <see cref="Unit"/> has a given <see cref="UnitSkill"/>.
        /// </summary>
        /// <typeparam name="TSkill">The type of skill to be found.</typeparam>
        /// <returns>True if this <see cref="Unit"/> has that <see cref="UnitSkill"/>, false if not.</returns>
        [Obsolete("Skills are being obsoleted, use components instead, or HasComponent<TComponent, TSkill> to aid transition.")]
        public bool HasSkill<TSkill>() where TSkill : UnitSkill
        {
            return InternalHasSkill<TSkill>();
        }

        [Obsolete("Skills are being obsoleted, use components instead.")]
        public bool HasSkill(Type skillType)
        {
            Argument.EnsureNotNull(skillType, "skillType");
            return skills.ContainsKey(skillType);
        }

        [Obsolete("Skills are being obsoleted, use components instead.")]
        public TSkill TryGetSkill<TSkill>() where TSkill : UnitSkill
        {
            return InternalTryGetSkill<TSkill>();
        }

        [Obsolete("Skills are being obsoleted, use components instead.")]
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
        [Obsolete("Skills are being obsoleted, use components instead.")]
        public int GetStat(UnitStat stat)
        {
            return Faction.GetStat(Type, stat);
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
            Debug.Assert(HasComponent<Attacker, AttackSkill>());

            int range = GetStat(AttackSkill.RangeStat);
            if (range == 0)
            {
                Debug.Assert(Components.Has<Spatial>(), "Unit has no spatial component!");
                Debug.Assert(other.Components.Has<Spatial>(), "Enemy unit has no spatial component!");
                bool selfIsAirborne = Components.Get<Spatial>().CollisionLayer == CollisionLayer.Air;
                bool otherIsAirborne = Components.Get<Spatial>().CollisionLayer == CollisionLayer.Air;
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

            Debug.Assert(unit.Components.Has<Spatial>(), "Unit has no spatial component!");
            bool isGroundUnit = unit.Components.Get<Spatial>().CollisionLayer == CollisionLayer.Ground;
            return HasSkill<TransportSkill>()
                && unit.HasComponent<Move, MoveSkill>()
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

            Debug.Assert(Components.Has<Spatial>(), "Unit has no Spatial component!");
            Components.Get<Spatial>().Angle = (float)Math.Atan2(delta.Y, delta.X);
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
            if (Components.Get<Attacker>().TimeElapsedSinceLastHit < hitDelayInSeconds)
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
            ArmorType targetArmorType = (ArmorType)target.GetStat(BasicSkill.ArmorTypeStat);

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

            Components.Get<Attacker>().TimeElapsedSinceLastHit = 0;
        }

        public bool IsSuperEffectiveAgainst(ArmorType type)
        {
            AttackSkill skill = Type.TryGetSkill<AttackSkill>();
            if (skill == null)
                return false;
            else
                return skill.IsSuperEffectiveAgainst(type);
        }

        public bool IsIneffectiveAgainst(ArmorType type)
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
            Faction.OnBuildingConstructionCompleted(this);
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
            TaskQueue.Clear();
            base.OnDied();
            Faction.OnUnitDied(this);
        }
        #endregion

        #region Updating
        protected override void DoUpdate(SimulationStep step)
        {
            // OPTIM: As checking for nearby units takes a lot of processor time,
            // we only do it once every few frames. We take our handle value
            // so the units do not make their checks all at once.
            if (CanPerformProximityChecks(step) && IsIdle)
            {
                if (HasSkill<SuicideBombSkill>() && TryExplodeWithNearbyUnit())
                    return;

                if (HasComponent<Builder, BuildSkill>() && TryRepairNearbyUnit()) { }
                else if (HasSkill<HealSkill>() && TryHealNearbyUnit()) { }
                else if (!IsUnderConstruction && HasComponent<Attacker, AttackSkill>() && !HasComponent<Builder, BuildSkill>()
                    && TryAttackNearbyUnit()) { }
            }
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

            bool isGroundUnit = Components.Get<Spatial>().CollisionLayer == CollisionLayer.Ground;
            if (!isGroundUnit && GetStat(AttackSkill.RangeStat) == 0)
                attackableUnits = attackableUnits.Where(u => u.Components.Get<Spatial>().CollisionLayer == CollisionLayer.Ground);

            // HACK: Attack units which can attack first, then other units.
            Unit unitToAttack = attackableUnits
                .WithMinOrDefault(unit => (unit.Position - Position).LengthSquared
                    + (unit.HasComponent<Attacker, AttackSkill>() ? 0 : 100));

            if (unitToAttack == null) return false;
        
            AttackTask attackTask = new AttackTask(this, unitToAttack);
            TaskQueue.Enqueue(attackTask);
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
            TaskQueue.OverrideWith(healTask);
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
                   && unit.Faction == Faction
                   && IsInLineOfSight(unit))
               .WithMinOrDefault(unit => (unit.Position - Position).LengthSquared);

            if (unitToRepair == null) return false;

            RepairTask repairTask = new RepairTask(this, unitToRepair);
            TaskQueue.OverrideWith(repairTask);
            return true;
        }
        #endregion

        public override string ToString()
        {
            return "{0} {2} {1}".FormatInvariant(Handle, Name, Faction);
        }
        #endregion
    }
}

