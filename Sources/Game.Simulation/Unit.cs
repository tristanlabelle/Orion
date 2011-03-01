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
using HealthComponent = Orion.Game.Simulation.Components.Health;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Represents an in-game unit, which can be a character, a vehicle or a building,
    /// depending on its <see cref="Entity"/>.
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

            // Mandatory components
            Identity identity = new Identity(this);
            identity.Name = type.Name;
            identity.VisualIdentity = type.GraphicsTemplate ?? type.Name;
            identity.SoundIdentity = type.VoicesTemplate ?? type.Name;
            identity.LeavesRemains = true;
            identity.IsSelectable = true;
            identity.Prototype = this;
            foreach (UnitTypeUpgrade upgrade in type.Upgrades)
                identity.Upgrades.Add(upgrade);
            identity.TrainType = InternalHasSkill<MoveSkill>() ? TrainType.Immaterial : TrainType.OnSite;
            identity.AladdiumCost = type.BasicSkill.AladdiumCost;
            identity.AlageneCost = type.BasicSkill.AlageneCost;
            Components.Add(identity);

            Spatial spatial = new Spatial(this);
            spatial.CollisionLayer = type.IsAirborne ? CollisionLayer.Air : CollisionLayer.Ground;
            spatial.Size = type.Size;
            Components.Add(spatial);

            Vision vision = new Vision(this);
            vision.Range = type.BasicSkill.SightRange;
            Components.Add(vision);

            Health health = new Health(this);
            health.MaximumValue = type.BasicSkill.MaxHealth;
            health.Constitution = InternalHasSkill<MoveSkill>() ? Constitution.Biological : Constitution.Mechanical;
            health.Armor = type.BasicSkill.Armor;
            health.ArmorType = type.BasicSkill.ArmorType;
            Components.Add(health);

            FactionMembership factionMembership = new FactionMembership(this);
            factionMembership.IsKeepAlive = InternalHasSkill<TrainSkill>() || InternalHasSkill<AttackSkill>();
            factionMembership.FoodCost = type.BasicSkill.FoodCost;
            if (InternalHasSkill<ProvideFoodSkill>())
            {
                ProvideFoodSkill provideFoodSkill = InternalTryGetSkill<ProvideFoodSkill>();
                factionMembership.ProvidedFood = provideFoodSkill.Amount;
            }
            Components.Add(factionMembership);

            Components.Add(new TaskQueue(this));

            // Optional components
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
                attacker.SuperEffectiveTargets.AddRange(attackSkill.SuperEffectiveAgainst);
                attacker.IneffectiveTargets.AddRange(attackSkill.IneffectiveAgainst);
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
                trainer.Speed = trainSkill.Speed;
                foreach (string target in trainSkill.Targets)
                    trainer.TrainableTypes.Add(target);
                Components.Add(trainer);
            }

            if (InternalHasSkill<ResearchSkill>())
            {
                ResearchSkill researchSkill = InternalTryGetSkill<ResearchSkill>();
                Researcher researcher = new Researcher(this);
                researcher.Technologies.AddRange(researcher.Technologies);
                Components.Add(researcher);
            }

            if (InternalHasSkill<HealSkill>())
            {
                HealSkill healSkill = InternalTryGetSkill<HealSkill>();
                Healer healer = new Healer(this);
                healer.Speed = healSkill.Speed;
                healer.Range = healSkill.Range;
                Components.Add(healer);
            }

            if (InternalHasSkill<TransportSkill>())
            {
                TransportSkill transportSkill = InternalTryGetSkill<TransportSkill>();
                Transporter transporter = new Transporter(this);
                transporter.Capacity = transportSkill.Capacity;
                Components.Add(transporter);
            }

            if (InternalHasSkill<StoreResourcesSkill>())
            {
                Components.Add(new ResourceDepot(this));
            }

            if (InternalHasSkill<ExtractAlageneSkill>())
            {
                Components.Add(new AlageneExtractor(this));
            }

            if (InternalHasSkill<SuicideBombSkill>())
            {
                SuicideBombSkill suicideBombSkill = InternalTryGetSkill<SuicideBombSkill>();
                Kamikaze kamikaze = new Kamikaze(this);
                kamikaze.Targets.AddRange(suicideBombSkill.Targets);
                kamikaze.Damage = suicideBombSkill.Damage;
                kamikaze.Radius = suicideBombSkill.Radius;
                Components.Add(kamikaze);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="Entity"/> from its identifier,
        /// <see cref="Entity"/> and <see cref="World"/>.
        /// </summary>
        /// <param name="handle">A unique handle for this <see cref="Entity"/>.</param>
        /// <param name="type">
        /// The <see cref="Entity"/> which determines
        /// the stats and capabilities of this <see cref="Entity"/>
        /// </param>
        /// <param name="position">The initial position of the <see cref="Entity"/>.</param>
        /// <param name="faction">The <see cref="Faction"/> this <see cref="Entity"/> is part of.</param>
        internal Unit(Handle handle, Unit prototype, Faction faction, Vector2 position)
            : base(faction.World, handle)
        {
            Argument.EnsureNotNull(prototype, "meta");
            Argument.EnsureNotNull(faction, "faction");

            skills = prototype.skills;

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
        /// Raised when the construction of this <see cref="Entity"/> is completed.
        /// </summary>
        public event Action<Unit> ConstructionCompleted;
        #endregion

        #region Properties
        #region Type-Related
        /// <summary>
        /// Temporary measure until we broadly use the identity component.
        /// </summary>
        public Unit Type
        {
            get { return this; }
            set
            {
                Argument.EnsureNotNull(value, "Type");

                Identity newTypeIdentity = value.Components.Get<Identity>();

                Identity identity = Components.Get<Identity>();
                identity.Name = value.Name;
                identity.VisualIdentity = newTypeIdentity.VisualIdentity;
                identity.SoundIdentity = newTypeIdentity.SoundIdentity;

                skills = value.skills;

                identity.Upgrades.Clear();
                foreach (UnitTypeUpgrade upgrade in value.Upgrades)
                    identity.Upgrades.Add(upgrade);
            }
        }

        public string Name
        {
            get { return Components.Get<Identity>().Name; }
        }

        public bool IsBuilding
        {
            get { return !HasComponent<Move, MoveSkill>(); }
        }

        public ICollection<UnitTypeUpgrade> Upgrades
        {
            get { return Components.Get<Identity>().Upgrades; }
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
        #endregion

        #region World & Faction
        public override bool IsAliveInWorld
        {
            get { return IsAlive; }
        }

        /// <summary>
        /// Accesses the <see cref="Faction"/> which this <see cref="Entity"/> is a member of.
        /// </summary>
        public Faction Faction
        {
            get { return Components.Get<FactionMembership>().Faction; }
        }
        #endregion

        #region Physical
        #region Health
        /// <summary>
        /// Accesses the damage that has been inflicted to this <see cref="Entity"/>, in health points.
        /// </summary>
        public float Damage
        {
            get { return Components.Get<Health>().Damage; }
            set { Components.Get<Health>().Damage = value; }
        }

        /// <summary>
        /// Gets the maximum amount of health points this <see cref="Entity"/> can have.
        /// </summary>
        public int MaxHealth
        {
            get { return (int)GetStatValue(HealthComponent.MaximumValueStat, BasicSkill.MaxHealthStat); }
        }

        /// <summary>
        /// Gets the amount of health points this <see cref="Entity"/> has.
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
        /// Accesses the time elapsed since this <see cref="Entity"/> last hit something, in seconds.
        /// </summary>
        public float TimeElapsedSinceLastHitInSeconds
        {
            get { return Components.Get<Attacker>().TimeElapsedSinceLastHit; }
        }
        #endregion
        #endregion

        #region Methods
        #region Skills/Type
        /// <summary>
        /// Tests if this <see cref="Entity"/> has a given <see cref="Component"/>,
        /// also checking if it has the corresponding <see cref="UnitSkill"/> to aid migration.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component to be found.</typeparam>
        /// <typeparam name="TSkill">The type of the corresponding skill.</typeparam>
        /// <returns>A value indicating if this <see cref="Entity"/> has a given <see cref="Component"/>.</returns>
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
        private TSkill InternalTryGetSkill<TSkill>() where TSkill : UnitSkill
        {
            UnitSkill skill;
            skills.TryGetValue(typeof(TSkill), out skill);
            return skill as TSkill;
        }

        [Obsolete("Skills are being obsoleted, use components instead.")]
        public int GetBaseStat(UnitStat stat)
        {
            Argument.EnsureNotNull(stat, "stat");

            UnitSkill skill;
            // As skills are being phased out, return 0 instead of throwing an exception
            return skills.TryGetValue(stat.SkillType, out skill) ? skill.GetStat(stat) : 0;
        }

        /// <summary>
        /// Obtains the value of a given <see cref="Stat"/>,
        /// asserting that the corresponding <see cref="UnitStat"/> has the same value.
        /// </summary>
        /// <param name="componentStat">The <see cref="Stat"/> being retrieved.</param>
        /// <param name="skillStat">The corresponding <see cref="UnitStat"/>.</param>
        /// <returns>The value of the <see cref="Stat"/>.</returns>
        public StatValue GetStatValue(Stat componentStat, UnitStat skillStat)
        {
            Argument.EnsureNotNull(componentStat, "componentStat");
            Argument.EnsureNotNull(skillStat, "skillStat");

            StatValue componentValue = GetStatValue(componentStat);
            Faction faction = Faction;
            if (faction != null)
            {
                int skillValue = Faction.GetStat(this, skillStat);
                if (componentValue.RealValue != skillValue)
                {
                    string message = "Component stat {0} did not have the same value as skill stat {1}."
                        .FormatInvariant(componentStat.FullName, skillStat.FullName);
                    Debug.Fail(message);
                }
            }

            return componentValue;
        }

        #region Can*** Testing
        public bool CanBuild(Unit buildingType)
        {
            Argument.EnsureNotNull(buildingType, "buildingType");

            Builder builder = Components.TryGet<Builder>();
            return buildingType.IsBuilding
                && builder != null
                && builder.Supports(buildingType);
        }

        public bool CanTrain(Unit unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");

            Trainer trainer = Components.TryGet<Trainer>();
            return !unitType.IsBuilding
                && trainer != null
                && trainer.Supports(unitType);
        }

        public bool CanResearch(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");

            Researcher researcher = Components.TryGet<Researcher>();
            return researcher != null && researcher.Supports(technology);
        }
        #endregion
        #endregion

        #region Position/Angle
        protected override Vector2 GetPosition()
        {
            return Position;
        }
        #endregion

        #region Hitting
        internal void OnHitting(Unit target, float damage)
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
            float explosionRadius = (float)GetStatValue(Kamikaze.RadiusStat, SuicideBombSkill.RadiusStat);
            Circle explosionCircle = new Circle(Center, explosionRadius);

            World.OnExplosionOccured(explosionCircle);
            Suicide();

            Unit[] damagedUnits = World.Entities
                .Intersecting(explosionCircle)
                .OfType<Unit>()
                .Where(unit => unit != this && unit.IsAliveInWorld)
                .ToArray();

            float explosionDamage = (float)GetStatValue(Kamikaze.DamageStat, SuicideBombSkill.DamageStat);
            foreach (Unit damagedUnit in damagedUnits)
            {
                if (damagedUnit.HasComponent<Kamikaze, SuicideBombSkill>()) continue;
                float distanceFromCenter = (explosionCircle.Center - damagedUnit.Center).LengthFast;
                float damage = (1 - (float)Math.Pow(distanceFromCenter / explosionCircle.Radius, 5))
                    * explosionDamage;
                damagedUnit.Health -= damage;
            }

            foreach (Unit damagedUnit in damagedUnits)
            {
                if (!damagedUnit.HasComponent<Kamikaze, SuicideBombSkill>()) continue;
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
                if (HasComponent<Kamikaze, SuicideBombSkill>() && TryExplodeWithNearbyUnit())
                    return;

                if (HasComponent<Builder, BuildSkill>() && TryRepairNearbyUnit()) { }
                else if (HasComponent<Healer, HealSkill>() && TryHealNearbyUnit()) { }
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
            Kamikaze kamikaze = Components.Get<Kamikaze>();

            Unit explodingTarget = World.Entities
                .Intersecting(Rectangle.FromCenterSize(Center, new Vector2(3, 3)))
                .OfType<Unit>()
                .FirstOrDefault(unit => unit != this
                    && kamikaze.IsTarget(unit)
                    && Region.AreAdjacentOrIntersecting(GridRegion, unit.GridRegion));

            if (explodingTarget == null) return false;

            float explosionRadius = (float)GetStatValue(Kamikaze.RadiusStat, SuicideBombSkill.RadiusStat);
            Circle explosionCircle = new Circle((Center + explodingTarget.Center) * 0.5f, explosionRadius);

            explodingTarget.Suicide();
            Explode();

            return true;
        }

        private bool TryAttackNearbyUnit()
        {
            Entity target = Components.Get<Attacker>().FindVisibleTarget();
            if (target == null) return false;
        
            AttackTask attackTask = new AttackTask(this, target);
            TaskQueue.Enqueue(attackTask);
            return true;
        }

        private bool TryHealNearbyUnit()
        {
            Entity target = Components.Get<Healer>().FindVisibleTarget();
            if (target == null) return false;

            HealTask healTask = new HealTask(this, target);
            TaskQueue.OverrideWith(healTask);
            return true;
        }

        private bool TryRepairNearbyUnit()
        {
            Entity target = Components.Get<Builder>().FindVisibleRepairTarget();
            if (target == null) return false;

            RepairTask repairTask = new RepairTask(this, target);
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

