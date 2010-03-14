using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Orion
{
    /// <summary>
    /// Provides helper methods to deal with argument validation.
    /// </summary>
    public static class Argument
    {
        #region Equality
        #region Integers
        /// <summary>
        /// Ensures that an argument is equal to a value.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="referenceValue">
        /// the value to which <paramref name="value"/> should be equal.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> does not equal <paramref name="referenceValue"/>.
        /// </exception>
        public static void EnsureEqual(int value, int referenceValue, string name)
        {
            if (value == referenceValue) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value of {0} but got {1}.".FormatInvariant(referenceValue, value));
        }

        /// <summary>
        /// Ensures that an argument is not equal to a value.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="referenceValue">
        /// the value to which <paramref name="value"/> should be not equal.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> equals <paramref name="referenceValue"/>.
        /// </exception>
        public static void EnsureNotEqual(int value, int referenceValue, string name)
        {
            if (value != referenceValue) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value different to {0} but got {1}.".FormatInvariant(referenceValue, value));
        }
        #endregion

        #region Misc
        /// <summary>
        /// Ensures that an argument is equal to a value.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="referenceValue">
        /// the value to which <paramref name="value"/> should be equal.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> does not equal <paramref name="referenceValue"/>.
        /// </exception>
        public static void EnsureEqual<T>(T value, T referenceValue, string name)
        {
            if (EqualityComparer<T>.Default.Equals(value, referenceValue)) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value of {0} but got {1}.".FormatInvariant(referenceValue, value));
        }

        /// <summary>
        /// Ensures that an argument is not equal to a value.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="referenceValue">
        /// the value to which <paramref name="value"/> should not be equal.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> equals <paramref name="referenceValue"/>.
        /// </exception>
        public static void EnsureNotEqual<T>(T value, T referenceValue, string name)
        {
            if (!EqualityComparer<T>.Default.Equals(value, referenceValue)) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value different to {0} but got {1}.".FormatInvariant(referenceValue, value));
        }
        #endregion
        #endregion

        #region Range
        #region Positive/Negative
        #region Integers
        /// <summary>
        /// Ensures that an argument is positive.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is strictly negative.
        /// </exception>
        public static void EnsurePositive(int value, string name)
        {
            if (value >= 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a positive value but got {0}.".FormatInvariant(value));
        }

        /// <summary>
        /// Ensures that an argument is strictly positive.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is negative or zero.
        /// </exception>
        public static void EnsureStrictlyPositive(int value, string name)
        {
            if (value > 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a strictly positive value but got {0}.".FormatInvariant(value));
        }

        /// <summary>
        /// Ensures that an argument is negative.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is strictly positive.
        /// </exception>
        public static void EnsureNegative(int value, string name)
        {
            if (value <= 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a negative value but got {0}.".FormatInvariant(value));

        }

        /// <summary>
        /// Ensures that an argument is strictly negative.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is positive or zero.
        /// </exception>
        public static void EnsureStrictlyNegative(int value, string name)
        {
            if (value < 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a strictly negative value but got {0}.".FormatInvariant(value));
        }
        #endregion

        #region Floats
        /// <summary>
        /// Ensures that an argument is positive.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is strictly negative.
        /// </exception>
        public static void EnsurePositive(float value, string name)
        {
            if (value >= 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a positive value but got {0}.".FormatInvariant(value));
        }

        /// <summary>
        /// Ensures that an argument is strictly positive.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is negative or zero.
        /// </exception>
        public static void EnsureStrictlyPositive(float value, string name)
        {
            if (value > 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a strictly positive value but got {0}.".FormatInvariant(value));
        }

        /// <summary>
        /// Ensures that an argument is negative.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is strictly positive.
        /// </exception>
        public static void EnsureNegative(float value, string name)
        {
            if (value <= 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a negative value but got {0}.".FormatInvariant(value));

        }

        /// <summary>
        /// Ensures that an argument is strictly negative.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is positive or zero.
        /// </exception>
        public static void EnsureStrictlyNegative(float value, string name)
        {
            if (value < 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a strictly negative value but got {0}.".FormatInvariant(value));
        }
        #endregion

        #region Doubles
        /// <summary>
        /// Ensures that an argument is positive.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is strictly negative.
        /// </exception>
        public static void EnsurePositive(double value, string name)
        {
            if (value >= 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a positive value but got {0}.".FormatInvariant(value));
        }

        /// <summary>
        /// Ensures that an argument is strictly positive.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is negative or zero.
        /// </exception>
        public static void EnsureStrictlyPositive(double value, string name)
        {
            if (value > 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a strictly positive value but got {0}.".FormatInvariant(value));
        }

        /// <summary>
        /// Ensures that an argument is negative.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is strictly positive.
        /// </exception>
        public static void EnsureNegative(double value, string name)
        {
            if (value <= 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a negative value but got {0}.".FormatInvariant(value));

        }

        /// <summary>
        /// Ensures that an argument is strictly negative.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is positive or zero.
        /// </exception>
        public static void EnsureStrictlyNegative(double value, string name)
        {
            if (value < 0) return;
            throw new ArgumentOutOfRangeException(name,
                "Expected a strictly negative value but got {0}.".FormatInvariant(value));
        }
        #endregion
        #endregion

        #region Comparison
        #region int
        /// <summary>
        /// Ensures that an argument is greater or equal to a value.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMinimum">
        /// The inclusive minimum bound of the range of acceptable values.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is smaller than <paramref name="inclusiveMinimum"/>.
        /// </exception>
        public static void EnsureGreaterOrEqual(int value, int inclusiveMinimum, string name)
        {
            if (value >= inclusiveMinimum) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value greater or equal to {0} but got {1}."
                .FormatInvariant(inclusiveMinimum, value));
        }

        /// <summary>
        /// Ensures that an argument is greater than a value.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="exclusiveMinimum">
        /// The exclusive minimum bound of the range of acceptable values.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is smaller or equal to <paramref name="exclusiveMinimum"/>.
        /// </exception>
        public static void EnsureGreater(int value, int exclusiveMinimum, string name)
        {
            if (value > exclusiveMinimum) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value greater than {0} but got {1}."
                .FormatInvariant(exclusiveMinimum, value));
        }

        /// <summary>
        /// Ensures that an argument is lower or equal to a value.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMaximum">
        /// The inclusive maximum bound of the range of acceptable values.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is greater than <paramref name="inclusiveMinimum"/>.
        /// </exception>
        public static void EnsureLowerOrEqual(int value, int inclusiveMaximum, string name)
        {
            if (value <= inclusiveMaximum) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value lower or equal to {0} but got {1}."
                .FormatInvariant(inclusiveMaximum, value));
        }

        /// <summary>
        /// Ensures that an argument is lower than a value.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="exclusiveMaximum">
        /// The exclusive maximum bound of the range of acceptable values.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is greater or equal to <paramref name="exclusiveMaximum"/>.
        /// </exception>
        public static void EnsureLower(int value, int exclusiveMaximum, string name)
        {
            if (value < exclusiveMaximum) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value lower than {0} but got {1}."
                .FormatInvariant(exclusiveMaximum, value));
        }
        #endregion

        #region Generic
        /// <summary>
        /// Ensures that an argument is greater or equal to a value.
        /// </summary>
        /// <typeparam name="T">The type of the argument's value.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMinimum">
        /// The inclusive minimum bound of the range of acceptable values.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is smaller than <paramref name="inclusiveMinimum"/>.
        /// </exception>
        public static void EnsureGreaterOrEqual<T>(T value, T inclusiveMinimum, string name)
        {
            if (Comparer<T>.Default.Compare(value, inclusiveMinimum) >= 0) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value greater or equal to {0} but got {1}."
                .FormatInvariant(inclusiveMinimum, value));
        }

        /// <summary>
        /// Ensures that an argument is greater than a value.
        /// </summary>
        /// <typeparam name="T">The type of the argument's value.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="exclusiveMinimum">
        /// The exclusive minimum bound of the range of acceptable values.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is smaller or equal to <paramref name="exclusiveMinimum"/>.
        /// </exception>
        public static void EnsureGreater<T>(T value, T exclusiveMinimum, string name)
        {
            if (Comparer<T>.Default.Compare(value, exclusiveMinimum) > 0) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value greater than {0} but got {1}."
                .FormatInvariant(exclusiveMinimum, value));
        }

        /// <summary>
        /// Ensures that an argument is lower or equal to a value.
        /// </summary>
        /// <typeparam name="T">The type of the argument's value.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMaximum">
        /// The inclusive maximum bound of the range of acceptable values.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is greater than <paramref name="inclusiveMinimum"/>.
        /// </exception>
        public static void EnsureLowerOrEqual<T>(T value, T inclusiveMaximum, string name)
        {
            if (Comparer<T>.Default.Compare(value, inclusiveMaximum) <= 0) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value lower or equal to {0} but got {1}."
                .FormatInvariant(inclusiveMaximum, value));
        }

        /// <summary>
        /// Ensures that an argument is lower than a value.
        /// </summary>
        /// <typeparam name="T">The type of the argument's value.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="exclusiveMaximum">
        /// The exclusive maximum bound of the range of acceptable values.
        /// </param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is greater or equal to <paramref name="exclusiveMaximum"/>.
        /// </exception>
        public static void EnsureLower<T>(T value, T exclusiveMaximum, string name)
        {
            if (Comparer<T>.Default.Compare(value, exclusiveMaximum) < 0) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value lower than {0} but got {1}."
                .FormatInvariant(exclusiveMaximum, value));
        }
        #endregion
        #endregion

        #region Within
        /// <summary>
        /// Ensures that an argument is within an inclusive range.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMinimum">The inclusive minimum acceptable value.</param>
        /// <param name="inclusiveMaximum">The inclusive maximum acceptable value.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is outside the interval
        /// [<paramref name="inclusiveMinimum"/>, <paramref name="inclusiveMinimum"/>].
        /// </exception>
        public static void EnsureWithin(int value, int inclusiveMinimum, int inclusiveMaximum, string name)
        {
            if (value >= inclusiveMinimum && value <= inclusiveMaximum) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value in range [{0}, {1}] but got {2}."
                .FormatInvariant(inclusiveMinimum, inclusiveMaximum, value));
        }

        /// <summary>
        /// Ensures that an argument is within an inclusive range.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMinimum">The inclusive minimum acceptable value.</param>
        /// <param name="inclusiveMaximum">The inclusive maximum acceptable value.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is outside the interval
        /// [<paramref name="inclusiveMinimum"/>, <paramref name="inclusiveMinimum"/>].
        /// </exception>
        public static void EnsureWithin(long value, long inclusiveMinimum, long inclusiveMaximum, string name)
        {
            if (value >= inclusiveMinimum && value <= inclusiveMaximum) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value in range [{0}, {1}] but got {2}."
                .FormatInvariant(inclusiveMinimum, inclusiveMaximum, value));
        }

        /// <summary>
        /// Ensures that an argument is within an [inclusive, exclusive[ range.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMinimum">The inclusive minimum acceptable value.</param>
        /// <param name="exclusiveMaximum">The exclusive maximum acceptable value.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is outside the interval
        /// [<paramref name="inclusiveMinimum"/>, <paramref name="exclusiveMinimum"/>[.
        /// </exception>
        public static void EnsureWithinIE(int value, int inclusiveMinimum, int exclusiveMaximum, string name)
        {
            if (value >= inclusiveMinimum && value < exclusiveMaximum) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value in range [{0}, {1}[ but got {2}."
                .FormatInvariant(inclusiveMinimum, exclusiveMaximum, value));
        }

        /// <summary>
        /// Ensures that an argument is within an [inclusive, exclusive[ range.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMinimum">The inclusive minimum acceptable value.</param>
        /// <param name="exclusiveMaximum">The exclusive maximum acceptable value.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is outside the interval
        /// [<paramref name="inclusiveMinimum"/>, <paramref name="exclusiveMinimum"/>[.
        /// </exception>
        public static void EnsureWithinIE(long value, long inclusiveMinimum, long exclusiveMaximum, string name)
        {
            if (value >= inclusiveMinimum && value < exclusiveMaximum) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value in range [{0}, {1}[ but got {2}."
                .FormatInvariant(inclusiveMinimum, exclusiveMaximum, value));
        }

        /// <summary>
        /// Ensures that an argument is within an inclusive range.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMinimum">The inclusive minimum acceptable value.</param>
        /// <param name="inclusiveMaximum">The inclusive maximum acceptable value.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is outside the interval
        /// [<paramref name="inclusiveMinimum"/>, <paramref name="exclusiveMinimum"/>].
        /// </exception>
        public static void EnsureWithin(float value, float inclusiveMinimum, float inclusiveMaximum,
            string name)
        {
            if (value >= inclusiveMinimum && value <= inclusiveMaximum) return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value in range [{0}, {1}] but got {2}."
                .FormatInvariant(inclusiveMinimum, inclusiveMaximum, value));
        }

        /// <summary>
        /// Ensures that an argument is within an inclusive range.
        /// </summary>
        /// <typeparam name="T">The type of value to be tested.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="inclusiveMinimum">The inclusive minimum acceptable value.</param>
        /// <param name="inclusiveMaximum">The inclusive maximum acceptable value.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is outside the interval
        /// [<paramref name="inclusiveMinimum"/>, <paramref name="inclusiveMinimum"/>].
        /// </exception>
        public static void EnsureWithin<T>(T value, T inclusiveMinimum, T inclusiveMaximum, string name)
        {
            IComparer<T> comparer = Comparer<T>.Default;
            if (comparer.Compare(value, inclusiveMinimum) >= 0 && comparer.Compare(value, inclusiveMaximum) <= 0)
                return;

            throw new ArgumentOutOfRangeException(name,
                "Expected a value in range [{0}, {1}] but got {2}."
                .FormatInvariant(inclusiveMinimum, inclusiveMaximum, value));
        }
        #endregion

        #region NaN/Infinity/Etc
        /// <summary>
        /// Ensures that an argument has a value different to NaN.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="NotFiniteNumberException">
        /// Thrown if <paramref name="value"/> is <c>NaN</c>.
        /// </exception>
        public static void EnsureNotNaN(float value, string name)
        {
            if (!float.IsNaN(value)) return;

            throw new NotFiniteNumberException(
                "Expected a non-NaN value for argument '{0}' but got {1}.".FormatInvariant(name, value),
                value);
        }

        /// <summary>
        /// Ensures that an argument has a value which is finite and not NaN.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="NotFiniteNumberException">
        /// Thrown if <paramref name="value"/> is infinite or NaN.
        /// </exception>
        public static void EnsureFinite(float value, string name)
        {
            if (!float.IsInfinity(value) && !float.IsNaN(value)) return;

            throw new NotFiniteNumberException(
                "Expected a finite value for argument '{0}' but got {1}.".FormatInvariant(name, value),
                value);
        }
        #endregion

        /// <summary>
        /// Ensures that a range given by an index and a count is within a capacity.
        /// </summary>
        /// <param name="offset">The offset of the start of the range.</param>
        /// <param name="count">The number of items.</param>
        /// <param name="capacity">The maximum capacity.</param>
        /// <param name="offsetArgName">The name of the <paramref name="offset"/> argument.</param>
        /// <param name="countArgName">The name of the <paramref name="count"/> argument.</param>
        public static void EnsureValidRange(int offset, int count, int capacity, string offsetArgName, string countArgName)
        {
            EnsurePositive(offset, offsetArgName);
            EnsurePositive(count, countArgName);
            if (offset + count > capacity)
            {
                throw new ArgumentException(
                    "Invalid range. {0} ({1}) + {2} ({3}) exceeds capacity ({4})."
                    .FormatInvariant(offsetArgName, offset, countArgName, count, capacity));
            }
        }
        #endregion

        #region Within set
        /// <summary>
        /// Ensures that an argument has a value equal to one element of a sequence.
        /// </summary>
        /// <typeparam name="T">The type of value to be tested.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="referenceValues">The sequence of refrence values.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> is not found within <paramref name="referenceValues"/>.
        /// </exception>
        public static void EnsureIn<T>(T value, IEnumerable<T> referenceValues, string name)
        {
            Argument.EnsureNotNull(referenceValues, "referenceValues");
            if (referenceValues.Contains(value)) return;

            string commaSeparatedValues = referenceValues
                .Select(v => (v is IFormattable) ? ((IFormattable)v).ToStringInvariant() : v.ToString())
                .Aggregate(string.Empty, (a,b) => a + ", " + b)
                .Substring(2);

            throw new ArgumentException("Expected one of ({0}) but got {1}."
                .FormatInvariant(commaSeparatedValues, name));
        }
        #endregion

        #region Null
        #region EnsureNotNull
        /// <summary>
        /// Ensures that an argument is not null.
        /// </summary>
        /// <typeparam name="T">The type of object to be tested.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="value"/> is null.
        /// </exception>
        public static void EnsureNotNull<T>(T value, string name) where T : class
        {
            if (typeof(T).IsValueType) return;
            if (value != null) return;
            throw new ArgumentNullException(name, "Expected a non-null value but got null.");
        }
        #endregion

        #region EnsureNoneNull
        /// <summary>
        /// Ensures that no element of a sequnce argument is null.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence argument.</typeparam>
        /// <param name="sequence">The sequence to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="sequence"/>, or any element within it is null.
        /// </exception>
        public static void EnsureNoneNull<T>(IEnumerable<T> sequence, string name)
        {
            Argument.EnsureNotNull(sequence, name);
            if (typeof(T).IsValueType) return;

            int index = 0;
            foreach (T item in sequence)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(
                        "{0}[{1}]".FormatInvariant(name, index),
                        "Expected all sequence elements to be non null but element at index {0} was."
                        .FormatInvariant(index));
                }

                ++index;
            }
        }
        #endregion

        #region EnsureNotNullNorEmpty
        /// <summary>
        /// Ensures that a string argument is not null nor empty.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="value"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> is empty.
        /// </exception>
        public static void EnsureNotNullNorEmpty(string value, string name)
        {
            Argument.EnsureNotNull(value, name);
            if (value.Length > 0) return;
            throw new ArgumentException("Expected a non-empty string value.", name);
        }

        /// <summary>
        /// Ensures that a given sequence is not null nor empty.
        /// </summary>
        /// <typeparam name="TElement">The type of elements in the sequence.</typeparam>
        /// <param name="value">The sequence to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="value"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> is empty.
        /// </exception>
        public static void EnsureNotNullNorEmpty<TElement>(IEnumerable<TElement> value, string name)
        {
            Argument.EnsureNotNull(value, name);
            if (value.Any()) return;
            throw new ArgumentException("Expected a non-empty sequence value.", name);
        }
        #endregion

        #region EnsureNotNullNorBlank
        /// <summary>
        /// Ensures that a string argument is not null nor composed of only whitespaces.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="value"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> is blank.
        /// </exception>
        public static void EnsureNotNullNorBlank(string value, string name)
        {
            Argument.EnsureNotNull(value, name);

            if (name.All(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException("Expected a non-blank string value.", name);
            }
        }
        #endregion
        #endregion

        #region Type
        /// <summary>
        /// Ensures that an argument has a given type.
        /// </summary>
        /// <param name="obj">The object to be tested.</param>
        /// <param name="baseType">The expected base type.</param>
        /// <param name="name">The name of the argument</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="obj"/> is null.
        /// </exception>
        /// <exception cref="ArgumentTypeException">
        /// Thrown if <paramref name="obj"/> is not derived from <paramref name="baseType"/>.
        /// </exception>
        public static void EnsureBaseType(object obj, Type baseType, string name)
        {
            EnsureNotNull(obj, name);
            EnsureBaseTypeOrNull(obj, baseType, name);
        }

        /// <summary>
        /// Ensures that an argument is null or has a given type.
        /// </summary>
        /// <param name="obj">The object to be tested.</param>
        /// <param name="baseType">The expected base type.</param>
        /// <param name="name">The name of the argument</param>
        /// <exception cref="ArgumentTypeException">
        /// Thrown if <paramref name="obj"/> is not derived from <paramref name="baseType"/>.
        /// </exception>
        public static void EnsureBaseTypeOrNull(object obj, Type baseType, string name)
        {
            if (obj == null) return;
            if (baseType.IsAssignableFrom(obj.GetType())) return;

            throw new ArgumentException(
                "Expected argument '{0}' to have type '{1}' but got '{2}'."
                .FormatInvariant(name, obj.GetType().FullName, baseType.FullName));
        }

        /// <summary>
        /// Ensures that an argument has a given type.
        /// </summary>
        /// <typeparam name="T">The expected base type.</typeparam>
        /// <param name="obj">The object to be tested.</param>
        /// <param name="name">The name of the argument</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="obj"/> is null.
        /// </exception>
        public static void EnsureBaseType<T>(object obj, string name)
        {
            EnsureNotNull(obj, name);
            EnsureBaseTypeOrNull<T>(obj, name);
        }

        /// <summary>
        /// Ensures that an argument is null or has a given type.
        /// </summary>
        /// <typeparam name="T">The expected base type.</typeparam>
        /// <param name="obj">The object to be tested.</param>
        /// <param name="baseType">The expected base type.</param>
        /// <param name="name">The name of the argument</param>
        public static void EnsureBaseTypeOrNull<T>(object obj, string name)
        {
            if (obj == null || obj is T) return;

            throw new ArgumentException(
                "Expected argument '{0}' to have type '{1}' but got '{2}'."
                .FormatInvariant(name, obj.GetType().FullName, typeof(T).FullName));
        }
        #endregion

        #region Enumerants
        /// <summary>
        /// Ensures that an argument is within an inclusive range.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown if <paramref name="value"/> is not defined in its the enumeration type.
        /// </exception>
        public static void EnsureDefined(Enum value, string name)
        {
            if (Enum.IsDefined(value.GetType(), value)) return;

            int integralValue = Convert.ToInt32(value, NumberFormatInfo.InvariantInfo);
            throw new InvalidEnumArgumentException(name, integralValue, value.GetType());
        }

        /// <summary>
        /// Ensures that an a flag of an enumeration is set.
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <param name="flag">The flag that should be set.</param>
        /// <param name="name">The name of the argument.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> does not specify <paramref name="flag"/>.
        /// </exception>
        public static void EnsureSet(Enum value, Enum flag, string name)
        {
            long integralValue = Convert.ToInt64(value, NumberFormatInfo.InvariantInfo);
            long integralFlagValue = Convert.ToInt64(flag, NumberFormatInfo.InvariantInfo);
            if ((integralValue & integralFlagValue) == integralFlagValue) return;

            throw new ArgumentException(
                "Expected enumerant with flag {0}, but got {1}.".FormatInvariant(flag, value),
                name);
        }
        #endregion
    }
}
