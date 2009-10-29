using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MoveSkill = Orion.GameLogic.Skills.Move;
using AttackSkill = Orion.GameLogic.Skills.Attack;
using HarvestAladdiumSkill = Orion.GameLogic.Skills.HarvestAladdium;
using BuildSkill = Orion.GameLogic.Skills.Build;

namespace Orion.GameLogic
{
    /// <summary>
    /// Describes a type of unit (including buildings and vehicles).
    /// </summary>
    [Serializable]
    public sealed class UnitType
    {
        #region Fields
		
        private readonly string name;
        private readonly TagSet tags = new TagSet();
        private readonly SkillCollection skills = new SkillCollection();

        // Base stats
        private readonly int aladdiumCost;
        private readonly int alageneCost;
        private readonly int maxHealth = 1;
        private readonly int sightRange = 5;

        // Dimensions
        private readonly int width = 1;
        private readonly int height = 1;
        private readonly int id;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="UnitType"/> from its name.
        /// </summary>
        /// <param name="name">The name of this <see cref="UnitType"/>.</param>
        public UnitType(string name, int id)
        {
            Argument.EnsureNotNullNorBlank(name, "name");

            this.name = name;
            this.id = id;
            // Temporarly hard-coded for backward compatibility.
            skills.Add(new MoveSkill(20));
            skills.Add(new AttackSkill(1, 2));
            skills.Add(new HarvestAladdiumSkill(10));
            skills.Add(new BuildSkill(unitType => true));
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        public int ID
        {
            get { return id; }
        } 

        /// <summary>
        /// Gets the name of this <see cref="UnitType"/>.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the set of tags associated with this <see cref="UnitType"/>.
        /// </summary>
        public TagSet Tags
        {
            get { return tags; }
        }

        public SkillCollection Skills
        {
            get { return skills; }
        }

        public bool IsBuilding
        {
            get { return name == "Building"; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }
        #endregion

        #region Methods
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
