using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic.Skills;
using Size = System.Drawing.Size;

namespace Orion.GameLogic
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
        private SkillCollection skills = new SkillCollection();
        private int aladdiumCost;
        private int alageneCost;
        private int maxHealth;
        private int meleeArmor;
        private int rangedArmor;
        private int sightRange;
        private int foodCost;
        private Size size;
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

        public SkillCollection Skills
        {
            get { return skills; }
        }

        public int AladdiumCost
        {
            get { return aladdiumCost; }
            set
            {
                Argument.EnsurePositive(value, "AladdiumCost");
                this.aladdiumCost = value;
            }
        }

        public int AlageneCost
        {
            get { return alageneCost; }
            set
            {
                Argument.EnsurePositive(value, "AlageneCost");
                this.alageneCost = value;
            }
        }

        public int MaxHealth
        {
            get { return maxHealth; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "MaxHealth");
                this.maxHealth = value;
            }
        }

        public int MeleeArmor
        {
            get { return meleeArmor; }
            set
            {
                Argument.EnsurePositive(value, "MeleeArmor");
                this.meleeArmor = value;
            }
        }

        public int RangedArmor
        {
            get { return rangedArmor; }
            set
            {
                Argument.EnsurePositive(value, "RangedArmor");
                this.rangedArmor = value;
            }
        }

        public int SightRange
        {
            get { return sightRange; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "SightRange");
                this.sightRange = value;
            }
        }

        public Size Size
        {
            get { return size; }
            set
            {
                Argument.EnsureStrictlyPositive(value.Width, "Size.Width");
                Argument.EnsureStrictlyPositive(value.Height, "Size.Height");
                this.size = value;
            }
        }

        public int FoodCost
        {
            get { return foodCost; }
            set 
            {
                Argument.EnsureStrictlyPositive(value, "FoodCost");
                this.foodCost = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets this builder to the default values.
        /// </summary>
        public void Reset()
        {
            name = null;
            skills.Clear();
            aladdiumCost = 0;
            alageneCost = 0;
            maxHealth = 1;
            meleeArmor = 0;
            rangedArmor = 0;
            sightRange = 1;
            foodCost = 0;
            size = new Size(1, 1);
        }

        public UnitType Build(Handle handle)
        {
            if (name == null) throw new InvalidOperationException("Cannot create a new UnitType until a name is set.");
            return new UnitType(handle, this);
        }
        #endregion
    }
}
