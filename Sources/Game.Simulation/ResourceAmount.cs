using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Orion.Engine;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Stores amounts of aladdium, alagene and food, and vector-like arithmetic on those values.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct ResourceAmount : IEquatable<ResourceAmount>
    {
        #region Instance
        #region Fields
        private readonly int aladdium;
        private readonly int alagene;
        private readonly int food;
        #endregion

        #region Constructors
        public ResourceAmount(int aladdium, int alagene, int food)
        {
            this.aladdium = aladdium;
            this.alagene = alagene;
            this.food = food;
        }

        public ResourceAmount(int aladdium, int alagene)
            : this(aladdium, alagene, 0) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the aladdium component of this amount, can be negative.
        /// </summary>
        public int Aladdium
        {
            get { return aladdium; }
        }

        /// <summary>
        /// Gets the alagene component of this amount, can be negative.
        /// </summary>
        public int Alagene
        {
            get { return alagene; }
        }

        /// <summary>
        /// Gets the food component of this amount, can be negative.
        /// </summary>
        public int Food
        {
            get { return food; }
        }
        #endregion

        #region Methods
        public int GetQuotient(ResourceAmount other)
        {
            int maximum = int.MaxValue;
            if (other.aladdium > 0) maximum = Math.Min(maximum, aladdium / other.aladdium);
            if (other.alagene > 0) maximum = Math.Min(maximum, alagene / other.alagene);
            if (other.food > 0) maximum = Math.Min(maximum, food / other.food);
            return maximum;
        }

        public bool Equals(ResourceAmount other)
        {
            return aladdium == other.aladdium
                && alagene == other.alagene
                && food == other.food;
        }

        public override bool Equals(object obj)
        {
            return obj is ResourceAmount && Equals((ResourceAmount)obj);
        }
        
        public override int GetHashCode()
        {
            return aladdium ^ (alagene << 12) ^ (food << 24);
        }

        public override string ToString()
        {
            return "{0} aladdium, {1} alagene and {2} food"
                .FormatInvariant(aladdium, alagene, food);
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A <see cref="ResourceAmount"/> which corresponds to zero aladdium, alagene and food.
        /// </summary>
        public static readonly ResourceAmount Zero = new ResourceAmount(0, 0, 0);
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new instance from the cost of a unit.
        /// </summary>
        /// <param name="unitType">The type of the unit.</param>
        /// <param name="faction">The faction to which the unit belongs.</param>
        /// <returns>A new instance based on the cost of that unit.</returns>
        public static ResourceAmount FromUnitCost(Unit unitType, Faction faction)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            Argument.EnsureNotNull(faction, "faction");

            int aladdiumCost = faction.GetStat(unitType, BasicSkill.AladdiumCostStat);
            int alageneCost = faction.GetStat(unitType, BasicSkill.AlageneCostStat);
            int foodCost = faction.GetStat(unitType, BasicSkill.FoodCostStat);

            return new ResourceAmount(aladdiumCost, alageneCost, foodCost);
        }

        public static bool Equals(ResourceAmount first, ResourceAmount second)
        {
            return first.Equals(second);
        }

        #region Arithmetic
        public static ResourceAmount Negate(ResourceAmount amount)
        {
            return new ResourceAmount(-amount.aladdium, -amount.alagene, -amount.food);
        }

        public static ResourceAmount Add(ResourceAmount first, ResourceAmount second)
        {
            return new ResourceAmount(
                first.aladdium + second.aladdium,
                first.alagene + second.alagene,
                first.food + second.food);
        }

        public static ResourceAmount Subtract(ResourceAmount first, ResourceAmount second)
        {
            return new ResourceAmount(
                first.aladdium - second.aladdium,
                first.alagene - second.alagene,
                first.food - second.food);
        }

        public static ResourceAmount Multiply(ResourceAmount amount, int ratio)
        {
            return new ResourceAmount(
                amount.aladdium * ratio,
                amount.alagene * ratio,
                amount.food * ratio);
        }
        #endregion
        #endregion

        #region Operators
        #region Comparison
        public static bool operator ==(ResourceAmount lhs, ResourceAmount rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(ResourceAmount lhs, ResourceAmount rhs)
        {
            return !Equals(lhs, rhs);
        }

        public static bool operator >=(ResourceAmount lhs, ResourceAmount rhs)
        {
            return lhs.aladdium >= rhs.aladdium
                && lhs.alagene >= rhs.alagene
                && lhs.food >= rhs.food;
        }

        public static bool operator <=(ResourceAmount lhs, ResourceAmount rhs)
        {
            return lhs.aladdium <= rhs.aladdium
                && lhs.alagene <= rhs.alagene
                && lhs.food <= rhs.food;
        }
        #endregion

        #region Arithmetic
        public static ResourceAmount operator +(ResourceAmount amount)
        {
            return amount;
        }

        public static ResourceAmount operator -(ResourceAmount amount)
        {
            return Negate(amount);
        }

        public static ResourceAmount operator +(ResourceAmount lhs, ResourceAmount rhs)
        {
            return Add(lhs, rhs);
        }

        public static ResourceAmount operator -(ResourceAmount lhs, ResourceAmount rhs)
        {
            return Subtract(lhs, rhs);
        }

        public static ResourceAmount operator *(ResourceAmount amount, int ratio)
        {
            return Multiply(amount, ratio);
        }
        #endregion
        #endregion
        #endregion
    }
}
