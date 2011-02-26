using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine.Geometry;
using Orion.Engine.Collections;

namespace Orion.Game.Simulation.Components
{
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

            Identity entityIdentity = entity.Components.TryGet<Identity>();
            return entity != Entity
                && entity.IsAliveInWorld
                && entityIdentity != null
                && targets.Contains(entityIdentity.Name);
        }
        #endregion
    }
}
