using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using System.IO;

namespace Orion.Commandment.Commands
{
    public sealed class SendRessources : Command
    { 
        #region Instance
        #region Fields
        private readonly Handle targetFactionHandle;
        private readonly int aladdiumAmount;
        private readonly int alageneAmount;
        #endregion

        #region Constructors
        public SendRessources(Handle factionHandle, Handle targetFactionHandle,
            int aladdiumAmount, int alageneAmount)
            : base(factionHandle)
        {
            this.targetFactionHandle = targetFactionHandle;
            this.aladdiumAmount = aladdiumAmount;
            this.alageneAmount = alageneAmount;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { yield break; }
        }
        #endregion

        #region Methods
        public override void Execute(World world)
        {
            Argument.EnsureNotNull(world, "world");

            Faction sendingFaction = world.FindFactionFromHandle(FactionHandle);
            Faction receivingFaction = world.FindFactionFromHandle(targetFactionHandle);

            if (sendingFaction.AladdiumAmount >= aladdiumAmount 
                && sendingFaction.AlageneAmount >= alageneAmount)
            {
                sendingFaction.AladdiumAmount -= aladdiumAmount;
                sendingFaction.AlageneAmount -= alageneAmount;
                receivingFaction.AladdiumAmount += aladdiumAmount;
                receivingFaction.AlageneAmount += alageneAmount;
            }
        }

        public override string ToString()
        {
            return "Faction {0} send {2} Aladdium and {3} Alagene to {1}."
                .FormatInvariant(FactionHandle, targetFactionHandle, aladdiumAmount, alageneAmount);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Attack"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<SendRessources>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(SendRessources command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteHandle(writer, command.targetFactionHandle);
                writer.Write(command.aladdiumAmount);
                writer.Write(command.alageneAmount);
            }

            protected override SendRessources DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                Handle targetFactionHandle = ReadHandle(reader);
                int aladdiumAmount = reader.ReadInt32();
                int alageneAmount = reader.ReadInt32();

                return new SendRessources(factionHandle, targetFactionHandle,
                    aladdiumAmount, alageneAmount);
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
