using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Technologies;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation
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
        private readonly bool isBuilding;
        private readonly Dictionary<Type, UnitSkill> skills;
        private readonly string heroName;
        #endregion

        #region Constructors
        internal UnitType(Handle handle, UnitTypeBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");

            this.handle = handle;
            this.name = builder.Name;

            this.size = builder.Size;
            this.isAirborne = builder.IsAirborne;

            this.skills = builder.Skills
                .Select(skill => skill.CreateFrozenClone())
                .ToDictionary(skill => skill.GetType());
            this.skills.Add(typeof(BasicSkill), builder.BasicSkill.CreateFrozenClone());

            this.isBuilding = !skills.ContainsKey(typeof(MoveSkill));
            this.heroName = builder.Hero;

#if DEBUG
            Debug.Assert(!HasSkill<AttackSkill>()
                || GetBaseStat(AttackSkill.RangeStat) <= GetBaseStat(BasicSkill.SightRangeStat),
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
                    return HasSkill<TrainSkill>();
                else
                    return HasSkill<BuildSkill>() || HasSkill<AttackSkill>();
            }
        }

        /// <summary>
        /// Gets the name of the unit type which is the hero of this unit.
        /// </summary>
        public string HeroName
        {
            get { return heroName; }
        }
        #endregion

        #region Methods
        #region Skills
        public bool HasSkill<TSkill>() where TSkill : UnitSkill
        {
            return skills.ContainsKey(typeof(TSkill));
        }

        public bool HasSkill(Type skillType)
        {
            Argument.EnsureNotNull(skillType, "skillType");
            return skills.ContainsKey(skillType);
        }

        public TSkill TryGetSkill<TSkill>() where TSkill : UnitSkill
        {
            UnitSkill skill;
            skills.TryGetValue(typeof(TSkill), out skill);
            return skill as TSkill;
        }

        public int GetBaseStat(UnitStat stat)
        {
            Argument.EnsureNotNull(stat, "stat");

            UnitSkill skill;
            if (!skills.TryGetValue(stat.SkillType, out skill))
            {
                throw new ArgumentException(
                    "Cannot get base stat {0} without skill {1}."
                    .FormatInvariant(stat, stat.SkillName));
            }

            return skill.GetStat(stat);
        }

        public bool CanBuild(UnitType buildingType)
        {
            Argument.EnsureNotNull(buildingType, "buildingType");
            BuildSkill skill = TryGetSkill<BuildSkill>();
            return buildingType.IsBuilding
                && skill != null
                && skill.Supports(buildingType);
        }

        public bool CanTrain(UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            TrainSkill skill = TryGetSkill<TrainSkill>();
            return !unitType.IsBuilding
                && skill != null
                && skill.Supports(unitType);
        }

        public bool CanResearch(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            ResearchSkill skill = TryGetSkill<ResearchSkill>();
            return skill != null
                && skill.Supports(technology);
        }
        #endregion

        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
