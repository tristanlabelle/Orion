﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// A <see cref="Component"/> which allows an <see cref="Entity"/>
    /// to execute healing tasks.
    /// </summary>
    public sealed class Healer : Component
    {
        #region Fields
        public static readonly Stat SpeedStat = new Stat(typeof(Healer), StatType.Real, "Speed");
        public static readonly Stat RangeStat = new Stat(typeof(Healer), StatType.Real, "Range");

        private float speed = 1;
        private float range = 1;
        #endregion

        #region Constructors
        public Healer(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the speed at which the host <see cref="Entity"/> heals.
        /// </summary>
        public float Speed
        {
            get { return speed; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "Speed");
                speed = value;
            }
        }

        /// <summary>
        /// Accesses the healing range of the host <see cref="Entity"/>.
        /// </summary>
        public float Range
        {
            get { return range; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "Range");
                range = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if a given <see cref="Entity"/> is within this <see cref="Entity"/>'s healing range.
        /// </summary>
        /// <param name="target">The target <see cref="Entity"/>.</param>
        /// <returns>A value indicating if <paramref name="target"/> is in the healing range.</returns>
        public bool IsInRange(Entity target)
        {
            Argument.EnsureNotNull(target, "target");

            Spatial spatial = Entity.Spatial;
            float range = (float)Entity.GetStatValue(Healer.RangeStat);
            return spatial != null && spatial.IsInRange(target, range);
        }
        #endregion
    }
}
