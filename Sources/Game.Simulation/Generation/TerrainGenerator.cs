using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Localization;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Abstract base class for terrain generation algorithms.
    /// </summary>
    public abstract class TerrainGenerator
    {
        #region Methods
        /// <summary>
        /// Gets the user-diplayable name of this terrain generator.
        /// </summary>
        /// <param name="localizer">A localizer to localize strings.</param>
        /// <returns>The name of this terrain generator.</returns>
        public abstract string GetName(Localizer localizer);

        /// <summary>
        /// Generates the tiles of a terrain.
        /// </summary>
        /// <param name="terrain">The terrain instance to be generated.</param>
        /// <param name="random">The random number generator to be used.</param>
        public abstract void Generate(Terrain terrain, Random random);
        #endregion
    }
}
