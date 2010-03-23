using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Provides a mutable representation of a <see cref="UnitType"/>
    /// as a way to create instances.
    /// </summary>
    [Serializable]
    public sealed class UnitTypeBuilder
    {
        #region Fields
        private string name;
        private Size size;
        private bool isAirborne;
        private readonly HashSet<UnitSkill> skills = new HashSet<UnitSkill>();
        private readonly Dictionary<UnitStat, int> stats = new Dictionary<UnitStat, int>();
        private readonly HashSet<string> buildTargets = new HashSet<string>();
        private readonly HashSet<string> trainTargets = new HashSet<string>();
        private readonly HashSet<string> researchTargets = new HashSet<string>();
        private readonly HashSet<string> suicideBombTargets = new HashSet<string>();
        #endregion

        #region Constructors
        public UnitTypeBuilder()
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
                this.name = value;
            }
        }

        public Size Size
        {
            get { return size; }
            set
            {
                Argument.EnsureWithin(value.Width, 0, Entity.MaxSize, "Size.Width");
                Argument.EnsureWithin(value.Height, 0, Entity.MaxSize, "Size.Height");
                this.size = value;
            }
        }

        public int Width
        {
            get { return size.Width; }
            set { size = new Size(value, size.Height); }
        }

        public int Height
        {
            get { return size.Height; }
            set { size = new Size(size.Width, value); }
        }

        public bool IsAirborne
        {
            get { return isAirborne; }
            set { isAirborne = value; }
        }

        public ICollection<UnitSkill> Skills
        {
            get { return skills; }
        }

        public IDictionary<UnitStat, int> Stats
        {
            get { return stats; }
        }

        public ICollection<string> TrainTargets
        {
            get { return trainTargets; }
        }

        public ICollection<string> BuildTargets
        {
            get { return trainTargets; }
        }

        public ICollection<string> ResearchTargets
        {
            get { return researchTargets; }
        }

        public ICollection<string> SuicideBombTargets
        {
            get { return suicideBombTargets; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets this builder to the default values.
        /// </summary>
        public void Reset()
        {
            name = null;
            size = new Size(1, 1);
            isAirborne = false;
            skills.Clear();
            stats.Clear();
        }

        public void Validate()
        {
            if (name == null) throw new InvalidOperationException("UnitType has no name.");

            foreach (UnitStat stat in stats.Keys)
            {
                if (!stat.HasAssociatedSkill) continue;

                UnitSkill skill = stat.AssociatedSkill;
                if (skills.Contains(skill)) continue;

                throw new InvalidOperationException("UnitType defines stat {0} without skill {1}."
                    .FormatInvariant(stat, skill));
            }

            foreach (UnitSkill skill in skills)
            {
                foreach (UnitStat stat in UnitStat.Values.Where(s => s.HasAssociatedSkill && s.AssociatedSkill == skill))
                {
                    if (!stats.ContainsKey(stat))
                    {
                        throw new InvalidOperationException("UnitTypes has skill {0} but does not define stat {1}."
                            .FormatInvariant(skills, stat));
                    }
                }
            }
        }

        public UnitType Build(Handle handle)
        {
            Validate();
            return new UnitType(handle, this);
        }
        #endregion
    }
}
