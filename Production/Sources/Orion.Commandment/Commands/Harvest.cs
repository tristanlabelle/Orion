using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using HarvestTask = Orion.GameLogic.Tasks.Harvest;

namespace Orion.Commandment.Commands
{
    public sealed class Harvest : Command
    {
        #region Instance
        #region Fields
        private readonly List<Unit> harvesters;
        private readonly ResourceNode node;
        #endregion

        #region Contructors
        public Harvest(Faction faction, IEnumerable<Unit> harvesters, ResourceNode node)
            :base(faction)
        {
            this.harvesters = harvesters.Distinct().ToList();
            this.node = node;
        }
        #endregion

        #region Properties
        public override IEnumerable<Entity> EntitiesInvolved
        {
            get
            {
                foreach (Unit unit in harvesters)
                    yield return unit;
                yield return node;
            }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit harvester in harvesters)
            {
                harvester.Task = new HarvestTask(harvester, node);
            }
        }

        public override string ToString()
        {
            return "[{0}] harvest {1}".FormatInvariant(harvesters.ToCommaSeparatedValues(), node);
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
                writer.Write(command.SourceFaction.ID);
                writer.Write(command.node.ID);
                writer.Write(command.harvesters.Count());
                foreach (Unit unit in command.harvesters)
                    writer.Write(unit.ID);
            }

            protected override Harvest DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                ResourceNode node = ReadResourceNode(reader, world);
                Unit[] units = ReadLengthPrefixedUnitArray(reader, world);
                return new Harvest(sourceFaction, units, node);
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
