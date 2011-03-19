using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Orion.Engine.Collections
{
    /// <summary>
    /// Provides access to prime numbers to be used in hashing algorithms.
    /// </summary>
    internal static class HashingPrimes
    {
        #region Fields
        private static readonly int[] primes = new int[]
        {
            // .NET primes
            3, 7, 11, 0x11, 0x17, 0x1d, 0x25, 0x2f, 0x3b, 0x47, 0x59, 0x6b, 0x83, 0xa3, 0xc5, 0xef, 
            0x125, 0x161, 0x1af, 0x209, 0x277, 0x2f9, 0x397, 0x44f, 0x52f, 0x63d, 0x78b, 0x91d, 0xaf1, 0xd2b, 0xfd1, 0x12fd, 
            0x16cf, 0x1b65, 0x20e3, 0x2777, 0x2f6f, 0x38ff, 0x446f, 0x521f, 0x628d, 0x7655, 0x8e01, 0xaa6b, 0xcc89, 0xf583, 0x126a7, 0x1619b, 
            0x1a857, 0x1fd3b, 0x26315, 0x2dd67, 0x3701b, 0x42023, 0x4f361, 0x5f0ed, 0x72125, 0x88e31, 0xa443b, 0xc51eb, 0xec8c1, 0x11bdbf, 0x154a3f, 0x198c4f, 
            0x1ea867, 0x24ca19, 0x2c25c1, 0x34fa1b, 0x3f928f, 0x4c4987, 0x5b8b6f, 0x6dda89,
            // Misc primes
            12582917,  25165843, 50331653,   100663319,  201326611, 402653189, 805306457, 1610612741
        };
        #endregion

        #region Methods
        /// <summary>
        /// Gets an array containing all hashing primes.
        /// </summary>
        /// <returns>A new array of hashing primes.</returns>
        public static int[] ToArray()
        {
            return (int[])primes.Clone();
        }

        /// <summary>
        /// Gets a hashing prime by its index.
        /// </summary>
        /// <param name="index">The index of the hashing prime.</param>
        /// <returns>The hashing prime at that index.</returns>
        public static int At(int index)
        {
            Argument.EnsurePositive(index, "index");
            return primes[index];
        }

        /// <summary>
        /// Gets the index of the next hashing prime over a given value.
        /// </summary>
        /// <param name="value">The value over which a hashing prime is to be found.</param>
        /// <returns>The index of the next hashing prime greater or equal to that value.</returns>
        public static int IndexOfNext(int value)
        {
            Argument.EnsurePositive(value, "value");

            for (int i = 0; i < primes.Length; ++i)
                if (primes[i] > value)
                    return i;

            throw new ArgumentOutOfRangeException("value");
        }

        /// <summary>
        /// Gets the next hashing prime over a given value.
        /// </summary>
        /// <param name="value">The value over which a hashing prime is to be found.</param>
        /// <returns>A hashing prime greater than that value.</returns>
        public static int GetNext(int value)
        {
            Argument.EnsurePositive(value, "value");

            for (int i = 0; i < primes.Length; ++i)
                if (primes[i] > value)
                    return primes[i];

            throw new ArgumentOutOfRangeException("value");
        }
        #endregion
    }
}
