using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Collections;
using Orion.GameLogic.Technologies;

namespace Orion.GameLogic
{
    /// <summary>
    /// Describes a type of unit (including buildings and vehicles).
    /// </summary>
    /// <remarks>
    /// Instances can be created through a <see cref="UnitTypeBuilder"/>.
    /// </remarks>
    [Serializable]
    public sealed class UnitType
    {
        #region Fields
        private readonly Handle handle;
        private readonly string name;

        private readonly Size size;
        private readonly bool isAirborne;

        private readonly HashSet<UnitSkill> skills;
        private readonly Dictionary<UnitStat, int> stats;
        private readonly HashSet<string> buildTargets;
        private readonly HashSet<string> trainTargets;
        private readonly HashSet<string> researchTargets;
        private readonly HashSet<string> suicideBombTargets;

        private readonly bool isBuilding;
        #endregion

        #region Constructors
        internal UnitType(Handle handle, UnitTypeBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");

            this.handle = handle;
            this.name = builder.Name;

            this.size = builder.Size;
            this.isAirborne = builder.IsAirborne;

            this.skills = new HashSet<UnitSkill>(builder.Skills);
            this.stats = new Dictionary<UnitStat, int>(builder.Stats);
            this.buildTargets = new HashSet<string>(builder.BuildTargets);
            this.trainTargets = new HashSet<string>(builder.TrainTargets);
            this.researchTargets = new HashSet<string>(builder.ResearchTargets);
            this.suicideBombTargets = new HashSet<string>(builder.SuicideBombTargets);

            var unspecifiedStatsWithDefaultValues = UnitStat.Values
                .Where(stat => stat.HasDefaultValue && !this.stats.ContainsKey(stat));
            foreach (UnitStat stat in unspecifiedStatsWithDefaultValues) this.stats.Add(stat, stat.DefaultValue);

            this.isBuilding = !skills.Contains(UnitSkill.Move);

#if DEBUG
            Debug.Assert(!skills.Contains(UnitSkill.Attack) || stats[UnitStat.AttackRange] <= stats[UnitStat.SightRange],
                "{0} has an attack range bigger than its line of sight.".FormatInvariant(name));
#endif
        }
        #endregion

        #region Properties
        #region Identification
        public Handle Handle
        {
            get { return handle; }
        } 

        public string Name
        {
            get { return name; }
        }
        #endregion

        #region Skills
        public bool IsBuilding
        {
            get { return isBuilding; }
        }

        public bool IsAirborne
        {
            get { return isAirborne; }
        }
        #endregion

        #region Size
        public Size Size
        {
            get { return size; }
        }

        public int Width
        {
            get { return size.Width; }
        }

        public int Height
        {
            get { return size.Height; }
        }
        #endregion

        public CollisionLayer CollisionLayer
        {
            get { return IsAirborne ? CollisionLayer.Air : CollisionLayer.Ground; }
        }

        /// <summary>
        /// Gets a value indicating if this type of unit keeps its <see cref="Faction"/> alive,
        /// that is, the <see cref="Faction"/> isn't defeated until all such units are dead.
        /// </summary>
        public bool KeepsFactionAlive
        {
            get
            {
                if (IsBuilding)
                    return HasSkill(UnitSkill.Train);
                else
                    return HasSkill(UnitSkill.Build) || HasSkill(UnitSkill.Attack);
            }
        }
        #endregion

        #region Methods
        public bool HasSkill(UnitSkill skill)
        {
            return skills.Contains(skill);
        }

        public int GetBaseStat(UnitStat stat)
        {
            int value;
            stats.TryGetValue(stat, out value);
            return value;
        }

        public bool CanBuild(UnitType targetType)
        {
            Argument.EnsureNotNull(targetType, "targetType");
            return HasSkill(UnitSkill.Build) && buildTargets.Contains(targetType.Name);
        }

        public bool CanTrain(UnitType targetType)
        {
            Argument.EnsureNotNull(targetType, "targetType");
            return HasSkill(UnitSkill.Train) && trainTargets.Contains(targetType.Name);
        }

        public bool CanResearch(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            return HasSkill(UnitSkill.Research) && researchTargets.Contains(technology.Name);
        }

        public bool IsSuicideBombTarget(UnitType targetType)
        {
            Argument.EnsureNotNull(targetType, "targetType");
            return HasSkill(UnitSkill.SuicideBomb) && suicideBombTargets.Contains(targetType.Name);
        }

        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
