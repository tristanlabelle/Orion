using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using HarvestTask = Orion.GameLogic.Tasks.Harvest;
using System.Collections.ObjectModel;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes the <see cref="HarvestTask"/>
    /// to be assigned to some <see cref="Unit"/>s.
    /// </summary>
    public sealed class Harvest : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> harvesterHandles;
        private readonly Handle resourceNodeHandle;
        #endregion

        #region Constructors
        public Harvest(Handle factionHandle, IEnumerable<Handle> harvestersHandles, Handle resourceNodeHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(harvestersHandles, "harvestersHandles");
            this.harvesterHandles = harvestersHandles.Distinct().ToList().AsReadOnly();
            this.resourceNodeHandle = resourceNodeHandle;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return harvesterHandles; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && harvesterHandles.All(handle => IsValidEntityHandle(world, handle))
                && IsValidEntityHandle(world, resourceNodeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            ResourceNode resourceNode = (ResourceNode)match.World.Entities.FromHandle(resourceNodeHandle);
            foreach (Handle harvesterHandle in harvesterHandles)
            {
                Unit harvester = (Unit)match.World.Entities.FromHandle(harvesterHandle);
                harvester.Task = new HarvestTask(harvester, resourceNode);
            }
        }

        public override string ToString()
        {
            return "[{0}] harvest {1}".FormatInvariant(harvesterHandles.ToCommaSeparatedValues(), resourceNodeHandle);
        }
        
        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, harvesterHandles);
            WriteHandle(writer, resourceNodeHandle);
        }

        public static Harvest DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var harvesterHandles = ReadLengthPrefixedHandleArray(reader);
            Handle resourceNodeHandle = ReadHandle(reader);
            return new Harvest(factionHandle, harvesterHandles, resourceNodeHandle);
        }
        #endregion
        #endregion
    }
}
