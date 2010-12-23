using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.Game.Simulation.Technologies
{
    /// <summary>
    /// Stores the technologies that can be developed by factions in a game.
    /// </summary>
    public sealed class TechnologyTree
    {
        #region Fields
        private readonly HashSet<Technology> technologies = new HashSet<Technology>();
        private readonly Func<Handle> handleGenerator = Handle.CreateGenerator();
        #endregion

        #region Constructors
        public TechnologyTree(AssetsDirectory assets)
        {
            foreach (string filePath in assets.EnumerateFiles("Technologies", "*.xml"))
            {
                try
                {
                    TechnologyBuilder builder = TechnologyReader.Read(filePath);
                    Handle handle = new Handle((uint)technologies.Count);
                    Technology technology = builder.Build(handle);
                    technologies.Add(technology);
                }
                catch (IOException e)
                {
                    Debug.Fail(
                        "Failed to read technology from file {0}:\n{1}"
                        .FormatInvariant(filePath, e));
                }
            }
        }
        #endregion

        #region Properties
        public IEnumerable<Technology> Technologies
        {
            get { return technologies; }
        }
        #endregion

        #region Methods
        public Technology FromHandle(Handle handle)
        {
            return technologies.FirstOrDefault(tech => tech.Handle == handle);
        }
        #endregion
    }
}
