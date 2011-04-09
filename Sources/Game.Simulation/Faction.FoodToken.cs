using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation
{
    partial class Faction
    {
        /// <summary>
        /// Describes the type of a <see cref="FoodToken"/>,
        /// specifying if it uses or provides food.
        /// </summary>
        [Serializable]
        public enum FoodTokenType
        {
            /// <summary>
            /// Specifies that the <see cref="FoodToken"/> uses food.
            /// </summary>
            Use,

            /// <summary>
            /// Specifies that the <see cref="FoodToken"/> provides food.
            /// </summary>
            Provide
        }

        /// <summary>
        /// Represents a use or provision of food to a <see cref="Faction"/>.
        /// </summary>
        [Serializable]
        public sealed class FoodToken : IDisposable
        {
            #region Fields
            private readonly Faction faction;
            private readonly FoodTokenType type;
            private int amount;
            #endregion

            #region Constructors
            internal FoodToken(Faction faction, FoodTokenType type, int amount)
            {
                Argument.EnsureNotNull(faction, "faction");
                Argument.EnsurePositive(amount, "amount");

                this.faction = faction;
                this.type = type;
                
                // Assign to the property so the faction's used food amount gets updated
                this.Amount = amount;
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the <see cref="Faction"/> involved with this <see cref="FoodToken"/>.
            /// </summary>
            public Faction Faction
            {
                get { return faction; }
            }

            /// <summary>
            /// Gets the type of this <see cref="FoodToken"/>.
            /// </summary>
            public FoodTokenType Type
            {
                get { return type; }
            }

            /// <summary>
            /// Accesses the amount of food involved by this <see cref="FoodToken"/>.
            /// </summary>
            public int Amount
            {
                get { return amount; }
                set
                {
                    Argument.EnsurePositive(value, "Amount");
                    if (value == amount) return;

                    int previousAmount = amount;
                    amount = value;

                    if (type == FoodTokenType.Use)
                    {
                        faction.usedFoodAmount -= previousAmount;
                        faction.usedFoodAmount += amount;
                        faction.UsedFoodAmountChanged.Raise(faction);
                    }
                    else
                    {
                        faction.totalFoodAmount -= previousAmount;
                        faction.totalFoodAmount += amount;
                        faction.MaxFoodAmountChanged.Raise(faction);
                    }
                }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Releases the food used by this <see cref="FoodToken"/>.
            /// </summary>
            public void Dispose()
            {
                Amount = 0;
            }

            public override string ToString()
            {
                string format = type == FoodTokenType.Use
                    ? "use {0} food from faction {1}"
                    : "provide {0} food to faction {1}";
                return format.FormatInvariant(amount, faction);
            }
            #endregion
        }
    }
}
