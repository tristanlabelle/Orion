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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mersenne
{
    /// <summary>
    /// The Mersenne Twister is a portable random number generator with a high period and decent performances (or at least we'll hope so,
    /// since we can't use vector instructions with the .net framework). Its prototype is slightly similar to the System.Random class.
    /// </summary>
    public class Twister
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
            /// Shifts a vector to the left by a specified number of BYTES.
            /// </summary>
            /// <param name="vec">The vector to shift</param>
            /// <param name="shift">The number of bytes to shift</param>
            /// <returns>The shifted vector</returns>
            public static Vector128 operator<<(Vector128 vec, int shift)
            {
                long vectorHigh = vec.c << 32 | vec.d;
                long vectorLow = vec.a << 32 | vec.b;

                long outputHigh = vectorHigh << (shift * 8);
                long outputLow = vectorLow << (shift * 8);
                outputLow |= vectorHigh >> (64 - shift * 8);

                return new Vector128((uint) outputLow >> 32, (uint) outputLow, (uint) outputHigh >> 32, (uint) outputHigh);
            }

            /// <summary>
            /// Shifts a vector to the right by a specified number of BYTES.
            /// </summary>
            /// <param name="vec">The vector to shift</param>
            /// <param name="shift">The number of bytes to shift</param>
            /// <returns>The shifted vector</returns>
            public static Vector128 operator>>(Vector128 vec, int shift)
            {
                long vectorHigh = vec.c << 32 | vec.d;
                long vectorLow = vec.a << 32 | vec.b;

                long outputHigh = vectorHigh >> (shift * 8);
                long outputLow = vectorLow >> (shift * 8);
                outputLow |= vectorHigh << (64 - shift * 8);

                return new Vector128((uint) outputLow >> 32, (uint) outputLow, (uint) outputHigh >> 32, (uint) outputHigh);
            }
        }
        #endregion

        #region Static Fields
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

        /// <summary>
        /// The number of bits to shift left when the vector is represented as four unsigned ints.
        /// </summary>
        private const int uintLeftShift = 18;

        /// <summary>
        /// The number of bytes to shift left when the vector is represented as a single 128 bits entity.
        /// </summary>
        private const int vectorLeftShift = 1;

        /// <summary>
        /// The number of bits to shift right when the vector is represented as four unsigned ints.
        /// </summary>
        private const int uintRightShift = 11;

        /// <summary>
        /// The number of bytes to shift right when the vector is represented as a single 128 bits entity.
        /// </summary>
        private const int vectorRightShift = 1;

        /// <summary>
        /// The masks are used to break the symmetry of SIMD instructions.
        /// </summary>
        private static readonly uint[] masks = { 0xdfffffef, 0xddfecb7f, 0xbffaffff, 0xbffffff6 };

        private static uint[] parityCheckArray = { 0x00000001, 0x00000000, 0x00000000, 0x13C9E684 };
        #endregion
        #endregion

        #region Fields
        /// <summary>
        /// The internal state array
        /// </summary>
        private Vector128[] state;

        /// <summary>
        /// The state array position
        /// </summary>
        private int vectorIndex;
        #endregion

        #region Constructors
        public Twister()
            : this(DateTime.Now.Second)
        { }

        public Twister(int seed)
        {
            state = new Vector128[N];
            state[0][0] = (uint)seed;
            for (int i = 1; i < ArraySize32; i++)
            {
                int prevIndex = i - 1;
                uint prev = state[prevIndex / 4][prevIndex % 4];
                state[i / 4][i % 4] = 1812433253 * (prev ^ (prev >> 30)) + 1;
            }
            vectorIndex = ArraySize32;
            CertificatePeriod();
        }
        #endregion

        #region Methods

        #region Pseudorandom Numbers Generation
        #region Int Generation
        public int Next()
        {
            if (vectorIndex >= ArraySize32)
            {
                RegenerateStateArray();
                vectorIndex = 0;
            }
            int random = (int) (state[vectorIndex / 4][vectorIndex % 4] & 0x7FFFFFFF);
            vectorIndex++;
            return random;
        }
        public int Next(int maxValue)
        {
            return Next() % maxValue;
        }

        public int Next(int minValue, int maxValue)
        {
            return Next() % (maxValue - minValue) + minValue;
        }
        #endregion

        #region Single Generation
        public float NextSingle()
        {
            return Next() / (float)0x7FFFFFFF;
        }

        public float NextSingle(float max)
        {
            return NextSingle() * max;
        }

        public float NextSingle(float min, float max)
        {
            return NextSingle() * (max - min) + min;
        }
        #endregion

        #region Double Generation
        public double NextDouble()
        {
            return Next() / (double)0x7FFFFFFF;
        }

        public double NextDouble(double max)
        {
            return NextDouble() * max;
        }

        public double NextDouble(double min, double max)
        {
            return NextDouble() * (max - min) + min;
        }
        #endregion
        #endregion

        #region Private Methods

        /// <summary>
        /// Fills the internal state array with pseudorandom integers
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

        private void CertificatePeriod()
        {
            uint inner = 0;
            int i;

            for (i = 0; i < 4; i++)
                inner ^= state[0][i] & parityCheckArray[i];

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
                    if ((work & parityCheckArray[i]) != 0)
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
            Vector128 x = a << vectorLeftShift;
            Vector128 y = c >> vectorRightShift;

            uint e = a[0] ^ x[0] ^ ((b[0] >> uintRightShift) & masks[0]) ^ y[0] ^ (d[0] << uintLeftShift);
            uint f = a[1] ^ x[1] ^ ((b[1] >> uintRightShift) & masks[1]) ^ y[1] ^ (d[1] << uintLeftShift);
            uint g = a[2] ^ x[2] ^ ((b[2] >> uintRightShift) & masks[2]) ^ y[2] ^ (d[1] << uintLeftShift);
            uint h = a[3] ^ x[3] ^ ((b[3] >> uintRightShift) & masks[3]) ^ y[3] ^ (d[2] << uintLeftShift);

            return new Vector128(e, f, g, h);
        }

        #endregion

        #endregion
    }
}
