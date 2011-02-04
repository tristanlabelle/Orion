/*
 * Copyright (c) 2006,2007 Mutsuo Saito, Makoto Matsumoto and Hiroshima
 * University. All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 * 
 *  * Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *  * Redistributions in binary form must reproduce the above
 *    copyright notice, this list of conditions and the following
 *    disclaimer in the documentation and/or other materials provided
 *    with the distribution.
 *  * Neither the name of the Hiroshima University nor the names of
 *    its contributors may be used to endorse or promote products
 *    derived from this software without specific prior written
 *    permission.
 *    
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;

namespace Orion.Engine
{
    /// <summary>
    /// The Mersenne Twister is a portable random number generator with a high period and decent performances (or at least we'll hope so,
    /// since we can't use vector instructions with the .net framework).
    /// </summary>
    public sealed class MersenneTwister : Random
    {
        #region Nested Types
        private struct Vector128
        {
            private uint a;
            private uint b;
            private uint c;
            private uint d;

            /// <summary>
            /// Returns a given part of the vector.
            /// </summary>
            /// <param name="index">The requested part (must range between 0 and 3)</param>
            /// <returns>The part of the vector</returns>
            public uint this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return a;
                        case 1: return b;
                        case 2: return c;
                        case 3: return d;
                    }
                    throw new ArgumentOutOfRangeException();
                }
                set
                {
                    if (index < 0 || index > 3) throw new ArgumentOutOfRangeException();
                    switch (index)
                    {
                        case 0: a = value; break;
                        case 1: b = value; break;
                        case 2: c = value; break;
                        case 3: d = value; break;
                    }
                }
            }

            /// <summary>
            /// Creates a 128 bits vector with four integers.
            /// </summary>
            /// <param name="a">The first integer</param>
            /// <param name="b">The second integer</param>
            /// <param name="c">The third integer</param>
            /// <param name="d">The fourth integer</param>
            public Vector128(uint a, uint b, uint c, uint d)
            {
                this.a = a;
                this.b = b;
                this.c = c;
                this.d = d;
            }

            /// <summary>
            /// Returns the binary or'ing of two vectors.
            /// </summary>
            /// <param name="v">
            /// A <see cref="Vector128"/>
            /// </param>
            /// <param name="w">
            /// A <see cref="Vector128"/>
            /// </param>
            /// <returns>
            /// The result of the two vectors being or'ed
            /// </returns>
            public static Vector128 operator |(Vector128 v, Vector128 w)
            {
                return new Vector128(v.a | w.a, v.b | w.b, v.c | w.c, v.d | w.d);
            }

            /// <summary>
            /// Returns the binary and'ing of two vectors.
            /// </summary>
            /// <param name="v">
            /// A <see cref="Vector128"/>
            /// </param>
            /// <param name="w">
            /// A <see cref="Vector128"/>
            /// </param>
            /// <returns>
            /// The result of the two vectors being or'ed
            /// </returns>
            public static Vector128 operator &(Vector128 v, Vector128 w)
            {
                return new Vector128(v.a & w.a, v.b & w.b, v.c & w.c, v.d & w.d);
            }

            /// <summary>
            /// Returns the binary xor'ing of two vectors.
            /// </summary>
            /// <param name="v">
            /// A <see cref="Vector128"/>
            /// </param>
            /// <param name="w">
            /// A <see cref="Vector128"/>
            /// </param>
            /// <returns>
            /// The result of the two vectors being or'ed
            /// </returns>
            public static Vector128 operator ^(Vector128 v, Vector128 w)
            {
                return new Vector128(v.a ^ w.a, v.b ^ w.b, v.c ^ w.c, v.d ^ w.d);
            }

            /// <summary>
            /// Shifts a vector to the left by a specified number of bits.
            /// </summary>
            /// <param name="vec">The vector to shift</param>
            /// <param name="shift">The number of bits to shift</param>
            /// <returns>The shifted vector</returns>
            public static Vector128 operator <<(Vector128 vec, int shift)
            {
                unchecked
                {
                    long high = vec.a << 32 | vec.b;
                    long low = vec.c << 32 | vec.d;
                    high <<= shift;
                    high |= low >> (sizeof(long) * 8 - shift);
                    low <<= shift;

                    return new Vector128((uint)high >> 32, (uint)high, (uint)low >> 32, (uint)low);
                }
            }

            /// <summary>
            /// Shifts a vector to the right by a specified number of bits.
            /// </summary>
            /// <param name="vec">The vector to shift</param>
            /// <param name="shift">The number of bits to shift</param>
            /// <returns>The shifted vector</returns>
            public static Vector128 operator >>(Vector128 vec, int shift)
            {
                unchecked
                {
                    long high = vec.a << 32 | vec.b;
                    long low = vec.c << 32 | vec.d;
                    low >>= shift;
                    low |= high << (sizeof(long) * 8 - shift);
                    high >>= shift;

                    return new Vector128((uint)high >> 32, (uint)high, (uint)low >> 32, (uint)low);
                }
            }
        }
        #endregion

        #region Static Fields
        /// <summary>
        /// Indicates the Mersenne Exponent (MEXP) used to generate numbers.
        /// </summary>
        public const int MersenneExponent = 19937;

        #region Private Static Fields
        /// <summary>
        /// The number of vectors in the internal state array.
        /// </summary>
        private const int N = MersenneExponent / 128 + 1;

        /// <summary>
        /// The minimum int array size required for it being filled by a Twister function.
        /// </summary>
        private const int ArraySize32 = N * 4;

        /// <summary>
        /// The pick up position of the array
        /// </summary>
        private const int PickupPosition = 122;

        private static readonly Vector128 maskVector = new Vector128(0xdfffffef, 0xddfecb7f, 0xbffaffff, 0xbffffff6);
        private static readonly Vector128 parityVector = new Vector128(0x00000001, 0x00000000, 0x00000000, 0x13C9E684);
        #endregion
        #endregion

        #region Fields
        /// <summary>
        /// The seed used to initialize this Mersenne Twister.
        /// </summary>
        public readonly int Seed;

        #region Private Fields
        /// <summary>
        /// The internal state array
        /// </summary>
        private readonly Vector128[] state;

        /// <summary>
        /// The state array position
        /// </summary>
        private int vectorIndex;
        #endregion
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new Mersenne Twister, using the current number of seconds since the beginning of the Unix Epoch as the seed.
        /// </summary>
        public MersenneTwister()
            : this(Environment.TickCount)
        { }

        /// <summary>
        /// Constructs a new Mersenne Twister using a passed <see cref="System.Int32"/> to initialize the generator.
        /// </summary>
        /// <param name="seed">
        /// A <see cref="System.Int32"/> used as the seed for the pseudorandom number generator
        /// </param>
        public MersenneTwister(int seed)
        {
            Seed = seed;

            state = new Vector128[N];
            state[0][0] = (uint)seed;
            for (int i = 1; i < ArraySize32; i++)
            {
                int prevIndex = i - 1;
                uint prev = state[prevIndex / 4][prevIndex % 4];
                state[i / 4][i % 4] = unchecked(1812433253 * (prev ^ (prev >> 30)) + 1);
            }
            vectorIndex = ArraySize32;
            CertifyPeriod();
        }
        #endregion

        #region Methods
        #region Pseudorandom Numbers Generation
        #region Int Generation
        /// <summary>
        /// Returns a positive pseudorandom <see cref="System.Int32"/> value comprised in the interval [0, 2^31-1).
        /// </summary>
        /// <returns>
        /// The pseudorandom <see cref="System.Int32"/>
        /// </returns>
        public override int Next()
        {
            if (vectorIndex >= ArraySize32)
            {
                RegenerateStateArray();
                vectorIndex = 0;
            }
            int random = (int)(state[vectorIndex / 4][vectorIndex % 4] & 0x7FFFFFFF);
            vectorIndex++;
            return random;
        }

        /// <summary>
        /// Returns a pseudorandom integer between 0 inclusively and the specified maximum value, exclusively. 
        /// </summary>
        /// <param name="maxValue">
        /// The exclusive maximum bound of the random number range.
        /// </param>
        /// <returns>
        /// The pseudorandom <see cref="System.Int32"/>
        /// </returns>
        /// <remarks>Passing a negative maxValue will still return positive integers because of how the modulo operator works in the .NET Framework.</remarks>
        public override int Next(int maxValue)
        {
            return Next() % maxValue;
        }

        /// <summary>
        /// Returns a pseudorandom integer in the range [minValue, maxValue).
        /// </summary>
        /// <param name="minValue">
        /// The minimum value for the pseudorandom integer
        /// </param>
        /// <param name="maxValue">
        /// The maximum value for the pseudorandom integer
        /// </param>
        /// <returns>
        /// A pseudorandom <see cref="System.Int32"/>
        /// </returns>
        public override int Next(int minValue, int maxValue)
        {
            return Next() % (maxValue - minValue) + minValue;
        }
        #endregion

        #region Single Generation
        /// <summary>
        /// Returns a pseudorandom <see cref="System.Single"/> in the range [0, 1).
        /// </summary>
        /// <returns>
        /// A pseudorandom <see cref="System.Single"/>
        /// </returns>
        public float NextSingle()
        {
            return Next() / (float)0x7FFFFFFF;
        }

        /// <summary>
        /// Returns a pseudorandom <see cref="System.Single"/> in the range [0, max).
        /// </summary>
        /// <param name="max">The maximum possible desired value</param>
        /// <returns>
        /// A pseudorandom <see cref="System.Single"/>
        /// </returns>
        public float NextSingle(float max)
        {
            return NextSingle() * max;
        }

        /// <summary>
        /// Returns a pseudorandom <see cref="System.Single"/> in the range [min, max).
        /// </summary>
        /// <param name="min">The minimum possible desired value</param>
        /// <param name="max">The maximum possible desired value</param>
        /// <returns>
        /// A pseudorandom <see cref="System.Single"/>
        /// </returns>
        public float NextSingle(float min, float max)
        {
            return NextSingle() * (max - min) + min;
        }
        #endregion

        #region Double Generation
        /// <summary>
        /// Returns a pseudorandom <see cref="System.Double"/> in the range [0, 1).
        /// </summary>
        /// <returns>
        /// A pseudorandom <see cref="System.Double"/>
        /// </returns>
        public override double NextDouble()
        {
            return Next() / (double)0x7FFFFFFF;
        }


        /// <summary>
        /// Returns a pseudorandom <see cref="System.Double"/> in the range [0, max).
        /// </summary>
        /// <param name="max">The maximum possible desired value</param>
        /// <returns>
        /// A pseudorandom <see cref="System.Double"/>
        public double NextDouble(double max)
        {
            return NextDouble() * max;
        }

        /// <summary>
        /// Returns a pseudorandom <see cref="System.Double"/> in the range [min, max).
        /// </summary>
        /// <param name="min">The minimum possible desired value</param>
        /// <param name="max">The maximum possible desired value</param>
        /// <returns>
        /// A pseudorandom <see cref="System.Double"/>
        /// </returns>
        public double NextDouble(double min, double max)
        {
            return NextDouble() * (max - min) + min;
        }
        #endregion

        #region Byte Buffer Generation
        public override void NextBytes(byte[] buffer)
        {
            Argument.EnsureNotNull(buffer, "buffer");

            for (int i = 0; i < buffer.Length; ++i)
                buffer[i] = (byte)Next(256);
        }
        #endregion
        #endregion

        #region Non-Public Methods
        protected override double Sample()
        {
            return NextDouble();
        }

        /// <summary>
        /// Fills the internal state array with pseudorandom integers.
        /// </summary>
        private void RegenerateStateArray()
        {
            int i;

            Vector128 r1 = state[N - 2];
            Vector128 r2 = state[N - 1];

            for (i = 0; i < N - PickupPosition; i++)
            {
                state[i] = DoRecursion(ref state[i], ref state[i + PickupPosition], ref r1, ref r2);
                r1 = r2;
                r2 = state[i];
            }

            while (i < N)
            {
                state[i] = DoRecursion(ref state[i], ref state[i + PickupPosition - N], ref r1, ref r2);
                r1 = r2;
                r2 = state[i];
                i++;
            }
        }

        private void CertifyPeriod()
        {
            uint inner = 0;
            int i;

            Vector128 parityState = state[0] & parityVector;
            for (i = 0; i < 4; i++)
                inner ^= parityState[i];

            for (i = 16; i > 0; i >>= 1)
                inner ^= inner >> i;
            inner &= 1;

            if (inner == 1)
                return;

            for (i = 0; i < 4; i++)
            {
                uint work = 1;
                for (int j = 0; j < 32; j++)
                {
                    if ((work & parityVector[i]) != 0)
                    {
                        state[0][i] ^= work;
                        return;
                    }
                    work <<= 1;
                }
            }
        }

        /// <summary>
        /// This method represents the recursion formula.
        /// </summary>
        /// <param name="a">a 128-bit part of the internal state array</param>
        /// <param name="b">a 128-bit part of the internal state array</param>
        /// <param name="c">a 128-bit part of the internal state array</param>
        /// <param name="d">a 128-bit part of the internal state array</param>
        /// <returns>Another vector</returns>
        private Vector128 DoRecursion(ref Vector128 a, ref Vector128 b, ref Vector128 c, ref Vector128 d)
        {
            return a ^ (a << 8) ^ ((b >> 11) & maskVector) ^ (c >> 8) ^ (d << 18);
        }

        #endregion
        #endregion
    }
}
