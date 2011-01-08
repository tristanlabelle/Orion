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
        public static readonly EntityStat<float> RadiusStat = new EntityStat<float>(typeof(Kamikaze), "Radius", "Rayon d'explosion");
        public static readonly EntityStat<int> DamageStat = new EntityStat<int>(typeof(Kamikaze), "Damage", "Dégâts");

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
