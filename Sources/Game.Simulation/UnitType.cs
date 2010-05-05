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
        private readonly string graphicsTemplate;
        private readonly string voicesTemplate;

        private readonly Size size;
        private readonly bool isAirborne;
        private readonly bool isSuicidable;
        private readonly bool isBuilding;
        private readonly Dictionary<Type, UnitSkill> skills;
        private readonly ReadOnlyCollection<UnitTypeUpgrade> upgrades;
        #endregion

        #region Constructors
        internal UnitType(Handle handle, UnitTypeBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");

            this.handle = handle;
            this.name = builder.Name;
            this.graphicsTemplate = builder.GraphicsTemplate ?? builder.Name;
            this.voicesTemplate = builder.VoicesTemplate ?? builder.Name;

            this.size = builder.Size;
            this.isAirborne = builder.IsAirborne;
            this.isSuicidable = builder.IsSuicidable;
            this.isBuilding = !builder.Skills.OfType<MoveSkill>().Any();

            this.skills = builder.Skills
                .Select(skill => skill.CreateFrozenClone())
                .ToDictionary(skill => skill.GetType());
            this.skills.Add(typeof(BasicSkill), builder.BasicSkill.CreateFrozenClone());
            this.upgrades = builder.Upgrades.ToList().AsReadOnly();

#if DEBUG
            Debug.Assert(!HasSkill<AttackSkill>()
                || GetBaseStat(AttackSkill.RangeStat) <= GetBaseStat(BasicSkill.SightRangeStat),
                "{0} has an attack range bigger than its line of sight.".FormatInvariant(name));
#endif
        }
        #endregion

        #region Properties
        public Handle Handle
        {
            get { return handle; }
        } 

        public string Name
        {
            get { return name; }
        }

        public string GraphicsTemplate
        {
            get { return graphicsTemplate; }
        }

        public string VoicesTemplate
        {
            get { return voicesTemplate; }
        }

        public bool IsBuilding
        {
            get { return isBuilding; }
        }

        public bool IsAirborne
        {
            get { return isAirborne; }
        }

        public bool IsSuicidable
        {
            get { return isSuicidable; }
        }

        public ReadOnlyCollection<UnitTypeUpgrade> Upgrades
        {
            get { return upgrades; }
        }

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
        #endregion

        #region Methods
        #region Skills & Stats
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
        #endregion
        
        #region Can*** Testing
        public bool CanTransport(UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            return HasSkill<TransportSkill>()
            	&& unitType.HasSkill<MoveSkill>()
            	&& !unitType.IsAirborne
            	&& !unitType.HasSkill<TransportSkill>();
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
