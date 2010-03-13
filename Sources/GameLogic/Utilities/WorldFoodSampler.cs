using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Utilities
{
    /// <summary>
    /// Keeps track of the food usage of each faction over time.
    /// </summary>
    public sealed class WorldFoodSampler
    {
        #region Fields
        private readonly World world;
        private readonly float samplingPeriod;
        private readonly Dictionary<Faction, List<int>> samples = new Dictionary<Faction,List<int>>();
        private float lastSimulationTime;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new world food monitor from the world to be sampled and the sampling period.
        /// </summary>
        /// <param name="world">The world to be sampled.</param>
        /// <param name="samplingPeriod">The amount of time, in seconds, between samples.</param>
        public WorldFoodSampler(World world, float samplingPeriod)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureStrictlyPositive(samplingPeriod, "samplingPeriod");
            
            this.world = world;
            this.samplingPeriod = samplingPeriod;
            foreach (Faction faction in world.Factions)
                samples.Add(faction, new List<int>());

            this.world.Updated += OnWorldUpdated;

            Sample();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the time between successive samples, in seconds.
        /// </summary>
        public float SamplingPeriod
        {
            get { return samplingPeriod; }
        }

        /// <summary>
        /// Gets the number of samples that have been done yet.
        /// </summary>
        public int SampleCount
        {
            get { return samples.First().Value.Count; }
        }

        /// <summary>
        /// Enumerates the factions for which samples are tracked.
        /// </summary>
        public IEnumerable<Faction> SampledFactions
        {
            get { return samples.Keys; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a sample of food usage for a given faction from its index.
        /// </summary>
        /// <param name="faction">The faction for which the samples are to be retrieved.</param>
        /// <param name="sampleIndex">The index of the sample.</param>
        /// <returns>The food usage of that faction on that sample.</returns>
        public int GetFactionFoodUsageSample(Faction faction, int sampleIndex)
        {
            Argument.EnsureNotNull(faction, "faction");
            return samples[faction][sampleIndex];
        }

        /// <summary>
        /// Gets a sample of food usage for a all world factions from its index.
        /// </summary>
        /// <param name="sampleIndex">The index of the sample.</param>
        /// <returns>The food usage of all world factions on that sample.</returns>
        public int GetWorldFoodUsageSample(int sampleIndex)
        {
            return samples.Values.Sum(factionSamples => factionSamples[sampleIndex]);
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            bool shouldSample = (int)(step.TimeInSeconds % samplingPeriod) != (int)(lastSimulationTime % samplingPeriod);
            lastSimulationTime = step.TimeInSeconds;
            if (!shouldSample) return;

            Sample();
        }

        private void Sample()
        {
            foreach (var entry in samples)
                entry.Value.Add(entry.Key.UsedFoodAmount);
        }
        #endregion
    }
}
