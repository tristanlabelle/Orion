using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Technologies
{
    /// <summary>
    /// Represents a technology that, when researched, has an effect
    /// on one or more stats of a <see cref="Entity"/>.
    /// </summary>
    /// <remarks>
    /// Possible requirements:
    /// - Resources
    /// - Other technologies (Not implemented)
    /// Possible effects:
    /// - Alter a stat of unit types with a tag
    /// - Unlock a unit type
    /// </remarks>
    [Serializable]
    public sealed class Technology
    {
        #region Fields
        private readonly Handle handle;
        private readonly string name;
        private readonly int aladdiumCost;
        private readonly int alageneCost;
        private readonly HashSet<string> targets;
        private readonly ReadOnlyCollection<TechnologyEffect> effects;
        #endregion

        #region Constructors
        internal Technology(Handle handle, TechnologyBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");

            this.handle = handle;
            this.name = builder.Name;
            this.aladdiumCost = builder.AladdiumCost;
            this.alageneCost = builder.AlageneCost;
            this.targets = new HashSet<string>(builder.Targets);
            this.effects = builder.Effects.ToList().AsReadOnly();

            Debug.Assert(this.targets.Count > 0, "Technology has no targets.");
            Debug.Assert(this.effects.Count > 0, "Technology has no effects.");
        }
        #endregion

        #region Properties
        public Handle Handle
        {
            get { return handle; }
        }

        /// <summary>
        /// Gets the name of this <see cref="Technology"/>.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        public int AladdiumCost
        {
            get { return aladdiumCost; }
        }

        public int AlageneCost
        {
            get { return alageneCost; }
        }

        /// <summary>
        /// Gets the sequence of this <see cref="Technology"/>'s effects.
        /// </summary>
        public IEnumerable<TechnologyEffect> Effects
        {
            get { return effects; }
        }
        #endregion

        #region Methods
        public bool AppliesTo(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            return targets.Contains(entity.Identity.Name);
        }

        public StatValue GetEffect(Entity entity, Stat stat)
        {
            Argument.EnsureNotNull(entity, "entity");
            
            StatValue sum = StatValue.CreateZero(stat.Type);
            if (!AppliesTo(entity)) return sum;

            foreach (TechnologyEffect effect in effects)
                if (effect.Stat == stat)
                    sum += effect.Delta;

            return sum;
        }

        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
