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
    /// Keeps the collection of registered <see cref="Entity"/>s prototypes.
    /// </summary>
    [Serializable]
    public sealed class PrototypeRegistry : IEnumerable<Entity>
    {
        #region Fields
        private readonly Dictionary<string, Entity> prototypes = new Dictionary<string, Entity>();
        #endregion

        #region Constructors
        public PrototypeRegistry(AssetsDirectory assets)
        {
            uint handle = 0;
            XmlDeserializer deserializer = new XmlDeserializer(() => new Handle(handle++));
            foreach (string filePath in assets.EnumerateFiles("Units", "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    Entity template = deserializer.DeserializeEntity(filePath, true);
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
        public void Register(Entity template)
        {
            Argument.EnsureNotNull(template, "template");
            prototypes.Add(template.Identity.Name, template);
        }

        public Entity FromHandle(Handle handle)
        {
            return prototypes.Values.FirstOrDefault(prototype => prototype.Handle == handle);
        }

        public Entity FromName(string name)
        {
            Entity type;
            prototypes.TryGetValue(name, out type);
            return type;
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return prototypes.Values.GetEnumerator();
        }
        #endregion

        #region Explicit Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
