using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Orion.Engine;
using System.Diagnostics;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Holds the value of a <see cref="Stat"/>.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct StatValue
    {
        #region Instance
        #region Fields
        private readonly StatType type;

        /// <summary>
        /// The value of the stat.
        /// </summary>
        /// <remarks>
        /// This is stored as a float even for integral values.
        /// This should not be a problem a floats can represent pretty much the whole range of shorts.
        /// </remarks>
        private readonly float value;
        #endregion

        #region Constructors
        private StatValue(StatType type, float value)
        {
            this.type = type;
            this.value = value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of this <see cref="StatValue"/>.
        /// </summary>
        public StatType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the integral value of this <see cref="StatValue"/>.
        /// </summary>
        public int IntegerValue
        {
            get
            {
                Debug.Assert(type == StatType.Integer, "Getting a real stat as an integer, the value will be truncated.");
                return (int)value;
            }
        }

        /// <summary>
        /// Gets the real value of this <see cref="StatValue"/>.
        /// </summary>
        public float RealValue
        {
            get { return value; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "{0} : {1}".FormatInvariant(value, type);
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        public static readonly StatValue IntegerZero = CreateZero(StatType.Integer);
        public static readonly StatValue RealZero = CreateZero(StatType.Real);
        #endregion

        #region Methods
        /// <summary>
        /// Creates a <see cref="StatValue"/> of a given type with a value of zero.
        /// </summary>
        /// <param name="type">The type of the stat.</param>
        /// <returns>A zero <see cref="StatValue"/>.</returns>
        public static StatValue CreateZero(StatType type)
        {
            return new StatValue(type, 0);
        }

        /// <summary>
        /// Creates an integral <see cref="StatValue"/>.
        /// </summary>
        /// <param name="value">The value of the stat.</param>
        /// <returns>The new integral <see cref="StatValue"/>.</returns>
        public static StatValue CreateInteger(int value)
        {
            return new StatValue(StatType.Integer, value);
        }

        /// <summary>
        /// Creates a real-number <see cref="StatValue"/>.
        /// </summary>
        /// <param name="value">The value of the stat.</param>
        /// <returns>The new real-number <see cref="StatValue"/>.</returns>
        public static StatValue CreateReal(float value)
        {
            return new StatValue(StatType.Real, value);
        }
        #endregion

        #region Operators
        public static StatValue operator +(StatValue a, StatValue b)
        {
            return new StatValue(a.type, a.value + b.value);
        }

        public static StatValue operator -(StatValue a, StatValue b)
        {
            return new StatValue(a.type, a.value - b.value);
        }

        public static explicit operator int(StatValue that)
        {
            return that.IntegerValue;
        }

        public static explicit operator float(StatValue that)
        {
            return that.RealValue;
        }
        #endregion
        #endregion
    }
}
