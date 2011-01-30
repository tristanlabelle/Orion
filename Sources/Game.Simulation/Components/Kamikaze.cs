using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine.Geometry;

namespace Orion.Game.Simulation.Components
{
    public class Kamikaze : Component
    {
        #region Static
        [SerializationReferenceable]
        public static bool WhenContact(Entity entity, string identity)
        {
            return entity.World.Entities
                .Intersecting(new Circle(entity.Center, (float)Math.Sqrt(2)))
                .Any(e => e.GetComponent<Identity>().Name == identity);
        }
        #endregion

        #region Instance
        #region Fields
        public static readonly EntityStat RadiusStat = new EntityStat(typeof(Kamikaze), StatType.Real, "Radius", "Rayon d'explosion");
        public static readonly EntityStat DamageStat = new EntityStat(typeof(Kamikaze), StatType.Integer, "Damage", "Dégâts");

        private Func<bool> shouldExplode;
        private float radius;
        private int damage;
        #endregion

        #region Constructors
        public Kamikaze(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public Func<bool> ShouldExplode
        {
            get { return shouldExplode; }
            set { shouldExplode = value; }
        }

        [Mandatory]
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        [Mandatory]
        public int Damage
        {
            get { return damage; }
            set { damage = value; }
        }
        #endregion
        #endregion
    }
}
