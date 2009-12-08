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
        private readonly int maxHealth;
        private readonly int meleeArmor;
        private readonly int rangedArmor;
        private readonly int sightRange;
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
            this.meleeArmor = builder.MeleeArmor;
            this.rangedArmor = builder.RangedArmor;

            var attackSkill = GetSkill<Skills.AttackSkill>();
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
            get { return !HasSkill<Skills.MoveSkill>(); }
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
                MoveSkill moveSkill = GetSkill<MoveSkill>();
                return moveSkill != null && moveSkill.IsAirborne;
            }
        }

        public CollisionLayer CollisionLayer
        {
            get { return IsAirborne ? CollisionLayer.Air : CollisionLayer.Ground; }
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
                    return HasSkill<TrainSkill>();
                else
                    return HasSkill<BuildSkill>() || HasSkill<AttackSkill>();
            }
        }
        #endregion

        #region Methods
        public TSkill GetSkill<TSkill>() where TSkill : Skill
        {
            // OPTIM: There was a linq query here, but as this method is called quite often
            // and had processor overhead, a for loop is preferred.
            for (int i = 0; i < skills.Count; ++i)
            {
                TSkill skill = skills[i] as TSkill;
                if (skill != null) return skill;
            }

            return null;
        }

        public bool HasSkill<TSkill>() where TSkill : Skill
        {
            return GetSkill<TSkill>() != null;
        }

        public int GetBaseStat(UnitStat stat)
        {
            switch (stat)
            {
                case UnitStat.AladdiumCost: return aladdiumCost;
                case UnitStat.AlageneCost: return alageneCost;
                case UnitStat.MaxHealth: return maxHealth;
                case UnitStat.SightRange: return sightRange;
                case UnitStat.MeleeArmor: return meleeArmor;
                case UnitStat.RangedArmor: return rangedArmor;
            }

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
