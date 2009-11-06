using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AttackSkill = Orion.GameLogic.Skills.Attack;
using BuildSkill = Orion.GameLogic.Skills.Build;
using HarvestSkill = Orion.GameLogic.Skills.Harvest;
using MoveSkill = Orion.GameLogic.Skills.Move;
using Size = System.Drawing.Size;
using OpenTK.Math;

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
        private readonly int id;
        private readonly string name;
        private readonly ReadOnlyCollection<Skill> skills;
        private readonly Size sizeInTiles;

        private readonly int aladdiumCost;
        private readonly int alageneCost;
        private readonly int maxHealth = 10;
        private readonly int sightRange = 10;
        #endregion

        #region Constructors
        internal UnitType(int id, UnitTypeBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");

            this.id = id;
            this.name = builder.Name;
            this.skills = builder.Skills.ToList().AsReadOnly();
            this.sizeInTiles = builder.SizeInTiles;

            this.aladdiumCost = builder.AladdiumCost;
            this.alageneCost = builder.AlageneCost;
            this.maxHealth = builder.MaxHealth;
            this.sightRange = builder.SightRange;
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        #region Identification
        public int ID
        {
            get { return id; }
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

        #region Size
        public Size SizeInTiles
        {
            get { return sizeInTiles; }
        }

        public int WidthInTiles
        {
            get { return sizeInTiles.Width; }
        }

        public int HeightInTiles
        {
            get { return sizeInTiles.Height; }
        }
        #endregion
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
