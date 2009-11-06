using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private int sightRange;
        private Size sizeInTiles;
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
                Argument.EnsurePositive(aladdiumCost, "aladdiumCost");
                this.aladdiumCost = value;
            }
        }

        public int AlageneCost
        {
            get { return alageneCost; }
            set
            {
                Argument.EnsurePositive(alageneCost, "alageneCost");
                this.alageneCost = value;
            }
        }

        public int MaxHealth
        {
            get { return maxHealth; }
            set
            {
                Argument.EnsureStrictlyPositive(maxHealth, "maxHealth");
                this.maxHealth = value;
            }
        }

        public int SightRange
        {
            get { return sightRange; }
            set
            {
                Argument.EnsureStrictlyPositive(sightRange, "sightRange");
                this.sightRange = value;
            }
        }

        public Size SizeInTiles
        {
            get { return sizeInTiles; }
            set
            {
                Argument.EnsureStrictlyPositive(value.Width, "SizeInTiles.Width");
                Argument.EnsureStrictlyPositive(value.Height, "SizeInTiles.Height");
                this.sizeInTiles = value;
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
            sightRange = 1;
            sizeInTiles = new Size(1, 1);
        }

        public UnitType Build(int id)
        {
            if (name == null) throw new InvalidOperationException("Cannot create a new UnitType until a name is set.");
            return new UnitType(id, this);
        }
        #endregion
    }
}
