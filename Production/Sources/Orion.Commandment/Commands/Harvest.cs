using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using HarvestTask = Orion.GameLogic.Tasks.Harvest;
using System.Collections.ObjectModel;

namespace Orion.Commandment.Commands
{
    public sealed class Harvest : Command
    {
        #region Instance
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
        public override void Execute(World world)
        {
            Argument.EnsureNotNull(world, "world");

            ResourceNode resourceNode = (ResourceNode)world.Entities.FindFromHandle(resourceNodeHandle);
            foreach (Handle harvesterHandle in harvesterHandles)
            {
                Unit harvester = (Unit)world.Entities.FindFromHandle(harvesterHandle);
                harvester.Task = new HarvestTask(harvester, resourceNode);
            }
        }

        public override string ToString()
        {
            return "[{0}] harvest {1}".FormatInvariant(harvesterHandles.ToCommaSeparatedValues(), resourceNodeHandle);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Cancel"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<Harvest>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Harvest command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteLengthPrefixedHandleArray(writer, command.harvesterHandles);
                WriteHandle(writer, command.resourceNodeHandle);
            }

            protected override Harvest DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                var harvesterHandles = ReadLengthPrefixedHandleArray(reader);
                Handle resourceNodeHandle = ReadHandle(reader);
                return new Harvest(factionHandle, harvesterHandles, resourceNodeHandle);
            }
            #endregion
            #endregion

            #region Static
            #region Fields
            /// <summary>
            /// A globally available static instance of this class.
            /// </summary>
            public static readonly Serializer Instance = new Serializer();
            #endregion
            #endregion
        }
        #endregion
    }
}
