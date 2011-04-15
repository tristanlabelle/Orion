using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine.Geometry;
using Orion.Engine.Collections;
using OpenTK;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Makes an <see cref="Entity"/> explode when entering in contact
    /// with some specific other <see cref="Entity">entities</see>.
    /// </summary>
    public sealed class Kamikaze : Component
    {
        #region Fields
        public static readonly Stat RadiusStat = new Stat(typeof(Kamikaze), StatType.Real, "Radius");
        public static readonly Stat DamageStat = new Stat(typeof(Kamikaze), StatType.Integer, "Damage");

        private readonly ICollection<string> targets
            = new ValidatingCollection<string>(new HashSet<string>(), value => value != null);
        private float radius;
        private int damage;
        #endregion

        #region Constructors
        public Kamikaze(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public ICollection<string> Targets
        {
            get { return targets; }
        }

        [Mandatory]
        public float Radius
        {
            get { return radius; }
            set
            {
                Argument.EnsurePositive(value, "Radius");
                radius = value;
            }
        }

        [Mandatory]
        public int Damage
        {
            get { return damage; }
            set
            {
                Argument.EnsurePositive(value, "Damage");
                damage = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if a given <see cref="Entity"/> provokes an explosion.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be tested.</param>
        /// <returns>A value indicating if the <see cref="Entity"/> is an explosion target.</returns>
        public bool IsTarget(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            Identity entityIdentity = entity.Identity;
            return entity != Entity
                && entity.IsAlive
                && entity.Spatial != null
                && entityIdentity != null
                && targets.Contains(entityIdentity.Name);
        }

        protected override void Update(SimulationStep step)
        {
            if (!Entity.CanPerformHeavyOperation) return;

            Spatial spatial = Entity.Spatial;
            if (spatial == null) return;

            Rectangle broadPhaseIntersectionRectangle = Rectangle.FromCenterSize(spatial.Center, (Vector2)spatial.Size + Vector2.One * 2);
            foreach (Spatial targetSpatial in World.SpatialManager.Intersecting(broadPhaseIntersectionRectangle))
            {
                Entity target = targetSpatial.Entity;
                if (target == Entity
                    || !IsTarget(target)
                    || !Region.AreAdjacentOrIntersecting(spatial.GridRegion, targetSpatial.GridRegion))
                {
                    continue;
                }

                ExplodeWith(target);
                return;
            }
        }

        private void ExplodeWith(Entity target)
        {
            Spatial spatial = Entity.Spatial;
            Spatial targetSpatial = target.Spatial;

            float explosionRadius = (float)Entity.GetStatValue(Kamikaze.RadiusStat);
            Circle explosionCircle = new Circle((spatial.Center + targetSpatial.Center) * 0.5f, explosionRadius);

            Health targetHealth = target.Components.TryGet<Health>();
            if (targetHealth != null) targetHealth.Suicide();

            Explode();
        }

        private void Explode()
        {
            Spatial spatial = Entity.Spatial;

            float explosionRadius = (float)Entity.GetStatValue(Kamikaze.RadiusStat);
            Circle explosionCircle = new Circle(spatial.Center, explosionRadius);

            World.OnExplosionOccured(explosionCircle);
            Entity.Components.Get<Health>().Suicide();

            var damagedEntities = World.SpatialManager
                .Intersecting(explosionCircle)
                .Select(s => s.Entity)
                .Where(entity => entity != Entity
                    && entity.IsAlive
                    && entity.Spatial != null
                    && entity.Components.Has<Health>())
                .NonDeferred();

            float explosionDamage = (float)Entity.GetStatValue(Kamikaze.DamageStat);
            foreach (Entity damagedEntity in damagedEntities)
            {
                if (damagedEntity.Components.Has<Kamikaze>()) continue;

                float distanceFromCenter = (explosionCircle.Center - damagedEntity.Spatial.Center).LengthFast;
                float damage = (1 - (float)Math.Pow(distanceFromCenter / explosionCircle.Radius, 5))
                    * explosionDamage;
                damagedEntity.Components.Get<Health>().Damage += damage;
            }

            foreach (Entity damagedEntity in damagedEntities)
            {
                Kamikaze kamikaze = damagedEntity.Components.TryGet<Kamikaze>();
                if (kamikaze == null) continue;

                kamikaze.Explode();
            }
        }
        #endregion
    }
}
