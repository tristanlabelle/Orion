using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Keeps the collection of registered <see cref="Entity"/>s.
    /// </summary>
    [Serializable]
    public sealed class UnitTypeRegistry : IEnumerable<Unit>
    {
        #region Fields
        private readonly Dictionary<string, Unit> types = new Dictionary<string, Unit>();
        #endregion

        #region Constructors
        public UnitTypeRegistry(AssetsDirectory assets)
        {
            uint handle = 0;
            XmlDeserializer deserializer = new XmlDeserializer(() => new Handle(handle++));
            foreach (string filePath in assets.EnumerateFiles("NewWorldUnits", "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    Unit template = deserializer.DeserializeEntity(filePath, true);
                    Register(template);
                }
                catch (IOException e)
                {
                    Debug.Fail(
                        "Failed to read unit type from file {0}:\n{1}"
                        .FormatInvariant(filePath, e));
                }
            }
        }
        #endregion

        #region Methods
        public Unit Register(Unit template)
        {
            Argument.EnsureNotNull(template, "template");
            types.Add(template.Identity.Name, template);
            return template;
        }

        public Unit FromHandle(Handle handle)
        {
            return types.Values.FirstOrDefault(unitType => unitType.Handle == handle);
        }

        public Unit FromName(string name)
        {
            Unit type;
            types.TryGetValue(name, out type);
            return type;
        }

        public IEnumerator<Unit> GetEnumerator()
        {
            return types.Values.GetEnumerator();
        }
        #endregion

        #region Explicit Members
        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #endregion
    }
}
