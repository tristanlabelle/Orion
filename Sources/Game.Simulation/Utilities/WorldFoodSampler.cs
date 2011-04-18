using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Engine;

namespace Orion.Game.Simulation.Utilities
{
    /// <summary>
    /// Keeps track of the food usage of each faction over time.
    /// </summary>
    public sealed class WorldFoodSampler
    {
        #region Fields
        private readonly World world;
        private readonly TimeSpan samplingPeriod;
        private readonly Dictionary<Faction, List<int>> samples = new Dictionary<Faction,List<int>>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new world food monitor from the world to be sampled and the sampling period.
        /// </summary>
        /// <param name="world">The world to be sampled.</param>
        /// <param name="samplingPeriod">The amount of time, between samples.</param>
        public WorldFoodSampler(World world, TimeSpan samplingPeriod)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureStrictlyPositive(samplingPeriod.Ticks, "samplingPeriod");
            
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
        public TimeSpan SamplingPeriod
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

        private TimeSpan SimulationTime
        {
            get { return world.SimulationTime; }
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

        /// <summary>
        /// Dumps the samples recorded by this sampler to a csv file.
        /// </summary>
        /// <param name="textWriter">The text writer to be used.</param>
        public void DumpCsv(TextWriter textWriter)
        {
            Argument.EnsureNotNull(textWriter, "textWriter");

            var firstFactionSamples = samples.First().Value;
            for (int i = 0; i < firstFactionSamples.Count; i++)
            {
                TimeSpan sampleTime = TimeSpan.FromTicks(i * samplingPeriod.Ticks);
                textWriter.Write(',');
                textWriter.Write(sampleTime.ToString());
            }
            textWriter.WriteLine();

            foreach (var entry in samples)
            {
                textWriter.Write(entry.Key.Name);
                foreach (int factionSample in entry.Value)
                {
                    textWriter.Write(',');
                    textWriter.Write(factionSample.ToStringInvariant());
                }
                textWriter.WriteLine();
            }
        }

        public void DumpCsv(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
                DumpCsv(writer);
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            bool shouldSample = (int)(step.TimeInSeconds / samplingPeriod.TotalSeconds)
                != (int)((step.TimeInSeconds - step.TimeDeltaInSeconds) / samplingPeriod.TotalSeconds);
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
