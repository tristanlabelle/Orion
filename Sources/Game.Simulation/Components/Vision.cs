using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components.Serialization;
using OpenTK;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Allows an <see cref="Entity"/> to have a line-of-sight radius.
    /// </summary>
    public sealed class Vision : Component
    {
        #region Fields
        public static readonly Stat RangeStat = new Stat(typeof(Vision), StatType.Integer, "Range");

        private int range = 1;
        #endregion

        #region Constructors
        public Vision(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the radius of the line-of-sight circle around this <see cref="Entity"/>.
        /// </summary>
        [Mandatory]
        public int Range
        {
            get { return range; }
            set
            {
                Argument.EnsurePositive(value, "Range");
                range = value;
            }
        }

        /// <summary>
        /// Gets the circle containing the part of the world seen by this <see cref="Entity"/>.
        /// </summary>
        [Transient]
        public Circle LineOfSight
        {
            get
            {
                Spatial spatial = Entity.Components.TryGet<Spatial>();
                Vector2 center = spatial == null ? new Vector2(float.NaN, float.NaN) : spatial.Center;
                return new Circle(center, (float)Entity.GetStatValue(RangeStat));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a value indicating if this <see cref="Entity"/> can see a given point.
        /// </summary>
        /// <param name="point">The point to be tested.</param>
        /// <returns><c>True</c> if the <see cref="point"/> can be seen, <c>false</c> if not.</returns>
        public bool CanSee(Vector2 point)
        {
            Spatial spatial = Entity.Components.TryGet<Spatial>();
            if (spatial == null) return false;

            Circle lineOfSight = new Circle(spatial.Center, (float)Entity.GetStatValue(RangeStat));
            return lineOfSight.ContainsPoint(point);
        }
        #endregion
    }
}
