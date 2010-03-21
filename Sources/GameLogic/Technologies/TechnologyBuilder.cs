using System;
using System.Collections.Generic;
using Orion.Engine;

namespace Orion.GameLogic.Technologies
{
    /// <summary>
    /// Provides a mutable equivalent to <see cref="Technology"/> that can be used to build such objects.
    /// </summary>
    [Serializable]
    public sealed class TechnologyBuilder
    {
        #region Fields
        private string name;
        private int aladdiumCost;
        private int alageneCost;
        private readonly HashSet<string> targets = new HashSet<string>();
        private readonly HashSet<TechnologyEffect> effects = new HashSet<TechnologyEffect>();
        #endregion

        #region Constructors
        public TechnologyBuilder()
        {
            Reset();
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNullNorBlank(value, "Name");
                name = value;
            }
        }

        public int AladdiumCost
        {
            get { return aladdiumCost; }
            set
            {
                Argument.EnsurePositive(value, "AladdiumCost");
                aladdiumCost = value;
            }
        }

        public int AlageneCost
        {
            get { return alageneCost; }
            set
            {
                Argument.EnsurePositive(value, "AlageneCost");
                alageneCost = value;
            }
        }

        public ICollection<string> Targets
        {
            get { return targets; }
        }

        public ICollection<TechnologyEffect> Effects
        {
            get { return effects; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets this <see cref="TechnologyBuilder"/>'s properties to their default values.
        /// </summary>
        public void Reset()
        {
            name = null;
            aladdiumCost = 0;
            alageneCost = 0;
            targets.Clear();
            effects.Clear();
        }

        /// <summary>
        /// Builds a <see cref="Technology"/> from this builder.
        /// </summary>
        /// <param name="handle">The handle of the newly built technology.</param>
        /// <returns>A newly created technology.</returns>
        public Technology Build(Handle handle)
        {
            if (name == null) throw new InvalidOperationException("A name must be set before the technology can be built.");

            return new Technology(handle, this);
        }
        #endregion
    }
}
