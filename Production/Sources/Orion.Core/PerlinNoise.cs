﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.Core
{
    [Serializable]
    public sealed class PerlinNoise
    {
        #region instance
        #region Fields
        private int r1 = 1000;
        private int r2 = 100000;
        private int r3 = 1000000000;

        private double frequency = 0.015;
        private double persistence = 0.65;
        private int octaves = 8;
        private double coverage = 0;
        private double amplitude = 1;
        private double density = 1;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new perlin noise generator using default settings.
        /// </summary>
        public PerlinNoise()
        {

        }

        /// <summary>
        /// Initialises and randomises a perlin noise generator.
        /// </summary>
        /// <param name="random">The random number generator to be used.</param>
        public PerlinNoise(MersenneTwister random)
        {
            Randomize(random);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the frequency parameter.
        /// </summary>
        public double Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }

        /// <summary>
        /// Accesses the persistence parameter.
        /// </summary>
        public double Persistence
        {
            get { return persistence; }
            set { persistence = value; }
        }

        /// <summary>
        /// Accesses the octaves parameter.
        /// </summary>
        public int Octaves
        {
            get { return octaves; }
            set { octaves = value; }
        }

        /// <summary>
        /// Accesses the coverage parameter.
        /// </summary>
        public double Coverage
        {
            get { return coverage; }
            set { coverage = value; }
        }

        /// <summary>
        /// Accesses the amplitude parameter.
        /// </summary>
        public double Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }

        /// <summary>
        /// Accesses the density parameter.
        /// </summary>
        public double Density
        {
            get { return density; }
            set { density = value; }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets the perlin noise value from the field at a given 1D coordinate.
        /// </summary>
        /// <param name="x">The x coordinate of the value in the field to be retrieved.</param>
        /// <returns>The value at that location.</returns>
        public double this[double x]
        {
            get { return this[x, 0]; }
        }

        /// <summary>
        /// Gets the value of the perlin noise field at a given coordinate.
        /// </summary>
        /// <param name="x">The x coordinate in the field.</param>
        /// <param name="y">The y coordinate in the field.</param>
        /// <returns>A value from the perlin noise field.</returns>
        public double this[double x, double y]
        {
            get
            {
                double total = 0.0;
                double currentAmplitude = amplitude;
                double currentFrequency = frequency;

                for (int octave = 0; octave < octaves; octave++)
                {
                    total += Smooth(x * currentFrequency, y * currentFrequency) * currentAmplitude;
                    currentFrequency *= 2;
                    currentAmplitude *= persistence;
                }

                total = (total + coverage) * density;

                if (total < 0) total = 0.0;
                if (total > 1) total = 1.0;

                return total;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Randomizes the field generated by this perlin noise generator.
        /// </summary>
        /// <param name="random">The random number generator to be used.</param>
        public void Randomize(MersenneTwister random)
        {
            Argument.EnsureNotNull(random, "random");
            r1 = random.Next(1000, 10000);
            r2 = random.Next(100000, 1000000);
            r3 = random.Next(1000000000, 2000000000);
        }

        private double Smooth(double x, double y)
        {
            double n1 = Noise((int)x, (int)y);
            double n2 = Noise((int)x + 1, (int)y);
            double n3 = Noise((int)x, (int)y + 1);
            double n4 = Noise((int)x + 1, (int)y + 1);

            double i1 = Interpolate(n1, n2, x - (int)x);
            double i2 = Interpolate(n3, n4, x - (int)x);

            return Interpolate(i1, i2, y - (int)y);
        }

        private double Noise(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;

            return (1.0 - ((n * (n * n * r1 + r2) + r3) & 0x7fffffff) / 1073741824.0);
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        private static double Interpolate(double x, double y, double a)
        {
            double val = (1 - Math.Cos(a * Math.PI)) * .5;
            return x * (1 - val) + y * val;
        }
        #endregion
        #endregion
    }
}
