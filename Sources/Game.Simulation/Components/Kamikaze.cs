using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    public class Kamikaze : Component
    {
        #region Fields
        public static readonly EntityStat RadiusStat = new EntityStat(typeof(Kamikaze), StatType.Real, "Radius", "Rayon d'explosion");
        public static readonly EntityStat DamageStat = new EntityStat(typeof(Kamikaze), StatType.Integer, "Damage", "Dégâts");

        private Func<bool> shouldExplode;
        private float radius;
        private int damage;
        #endregion

        #region Constructors
        public Kamikaze(Entity entity, Func<bool> shouldExplode, float radius, int damage)
            : base(entity)
        {
            Argument.EnsureNotNull(shouldExplode, "shouldExplode");
            this.shouldExplode = shouldExplode;
            this.radius = radius;
            this.damage = damage;
        }
        #endregion

        #region Properties
        public Func<bool> ShouldExplode
        {
            get { return shouldExplode; }
        }

        public float Radius
        {
            get { return radius; }
        }

        public int Damage
        {
            get { return damage; }
        }
        #endregion
    }
}
