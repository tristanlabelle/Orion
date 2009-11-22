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
        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Faction sendingFaction = match.World.FindFactionFromHandle(FactionHandle);
            Faction receivingFaction = match.World.FindFactionFromHandle(targetFactionHandle);

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

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteHandle(writer, targetFactionHandle);
            writer.Write(aladdiumAmount);
            writer.Write(alageneAmount);
        }

        public static SendRessources DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle targetFactionHandle = ReadHandle(reader);
            int aladdiumAmount = reader.ReadInt32();
            int alageneAmount = reader.ReadInt32();

            return new SendRessources(factionHandle, targetFactionHandle,
                aladdiumAmount, alageneAmount);
        }
        #endregion
        #endregion
    }
}
