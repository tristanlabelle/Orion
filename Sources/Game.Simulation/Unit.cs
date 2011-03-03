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
        /// <param name="faction">The <see cref="T:Faction"/> this <see cref="Entity"/> is part of.</param>
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

                Identity newTypeIdentity = value.Identity;

                Identity identity = Identity;
                identity.Name = newTypeIdentity.Name;
                identity.VisualIdentity = newTypeIdentity.VisualIdentity;
                identity.SoundIdentity = newTypeIdentity.SoundIdentity;

                skills = value.skills;

                identity.Upgrades.Clear();
                foreach (UnitTypeUpgrade upgrade in value.Upgrades)
                    identity.Upgrades.Add(upgrade);
            }
        }

        public bool IsBuilding
        {
            get { return !Components.Has<Move>(); }
        }

        public ICollection<UnitTypeUpgrade> Upgrades
        {
            get { return Identity.Upgrades; }
        }
        #endregion
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
            get { return (int)GetStatValue(HealthComponent.MaximumValueStat); }
        }

        /// <summary>
        /// Gets the amount of health points this <see cref="Entity"/> has.
        /// </summary>
        public float Health
        {
            get { return MaxHealth - Damage; }
            set { Damage = MaxHealth - value; }
        }
        #endregion

        #region Tasks
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

        internal void OnHitting(Unit target, float damage)
        {
            HitEventArgs args = new HitEventArgs(this, target, damage, World.LastSimulationStep.TimeInSeconds);

            World.OnUnitHitting(args);
        }

        internal void OnConstructionCompleted()
        {
            ConstructionCompleted.Raise(this);

            Faction faction = FactionMembership.GetFaction(this);
            if (faction != null) faction.OnBuildingConstructionCompleted(this);
        }
        #endregion
    }
}

