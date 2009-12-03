using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic.Skills;
using Size = System.Drawing.Size;
using System.Diagnostics;

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
        private readonly ReadOnlyCollection<Skill> skills;
        private readonly Size size;

        private readonly int aladdiumCost;
        private readonly int alageneCost;
        private readonly int maxHealth = 10;
        private readonly int sightRange = 10;
        private readonly int foodCost;
        #endregion

        #region Constructors
        internal UnitType(Handle handle, UnitTypeBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");

            this.handle = handle;
            this.name = builder.Name;
            this.skills = builder.Skills.ToList().AsReadOnly();
            this.size = builder.Size;

            this.aladdiumCost = builder.AladdiumCost;
            this.alageneCost = builder.AlageneCost;
            this.maxHealth = builder.MaxHealth;
            this.sightRange = builder.SightRange;
            this.foodCost = builder.FoodCost;

            var attackSkill = GetSkill<Skills.Attack>();
            Debug.Assert(attackSkill == null || attackSkill.MaxRange <= sightRange,
                "{0} has an attack range bigger than its line of sight.".FormatInvariant(name));
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
        public ReadOnlyCollection<Skill> Skills
        {
            get { return skills; }
        }

        public bool IsBuilding
        {
            get { return !HasSkill<Skills.Move>(); }
        }
        #endregion

        public Size Size
        {
            get { return size; }
        }

        public bool IsAirborne
        {
            get
            {
                Move moveSkill = GetSkill<Move>();
                return moveSkill != null && moveSkill.IsAirborne;
            }
        }

        public int FoodCost
        {
            get { return foodCost; }
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
                    return HasSkill<Train>();
                else
                    return HasSkill<Build>() || HasSkill<Attack>();
            }
        }
        #endregion

        #region Methods
        public TSkill GetSkill<TSkill>() where TSkill : Skill
        {
            return skills.OfType<TSkill>().FirstOrDefault();
        }

        public bool HasSkill<TSkill>() where TSkill : Skill
        {
            return GetSkill<TSkill>() != null;
        }

        public int GetBaseStat(UnitStat stat)
        {
            if (stat == UnitStat.AladdiumCost) return aladdiumCost;
            if (stat == UnitStat.AlageneCost) return alageneCost;
            if (stat == UnitStat.MaxHealth) return maxHealth;
            if (stat == UnitStat.SightRange) return sightRange;

            for (int i = 0; i < skills.Count; ++i)
            {
                int? value = skills[i].TryGetBaseStat(stat);
                if (value.HasValue) return value.Value;
            }

            return 0;
        }

        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
