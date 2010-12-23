using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Keeps the collection of registered <see cref="UnitType"/>s.
    /// </summary>
    [Serializable]
    public sealed class UnitTypeRegistry : IEnumerable<UnitType>
    {
        #region Fields
        private readonly Dictionary<string, UnitType> types = new Dictionary<string, UnitType>();
        #endregion

        #region Constructors
        public UnitTypeRegistry(AssetsDirectory assets)
        {
            foreach (string filePath in assets.EnumerateFiles("Units", "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    UnitTypeBuilder builder = UnitTypeReader.Read(filePath);
                    Register(builder);
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
        public UnitType Register(UnitTypeBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");
            UnitType unitType = builder.Build(new Handle((uint)types.Count));
            types.Add(unitType.Name, unitType);
            return unitType;
        }

        public UnitType FromHandle(Handle handle)
        {
            return types.Values.FirstOrDefault(unitType => unitType.Handle == handle);
        }

        public UnitType FromName(string name)
        {
            UnitType type;
            types.TryGetValue(name, out type);
            return type;
        }

        public IEnumerator<UnitType> GetEnumerator()
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
