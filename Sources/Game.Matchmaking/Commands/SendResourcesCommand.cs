using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes resources to be given to another <see cref="Faction"/>.
    /// </summary>
    public sealed class SendResourcesCommand : Command
    { 
        #region Fields
        private readonly Handle targetFactionHandle;
        private readonly int aladdiumAmount;
        private readonly int alageneAmount;
        #endregion

        #region Constructors
        public SendResourcesCommand(Handle factionHandle, Handle targetFactionHandle,
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
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && IsValidFactionHandle(match, targetFactionHandle);
        }

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
            return "Faction {0} sends {1} aladdium and {2} alagene to faction {3}"
                .FormatInvariant(FactionHandle, aladdiumAmount, alageneAmount, targetFactionHandle);
        }

        #region Serialization
        public static void Serialize(SendResourcesCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteHandle(writer, command.targetFactionHandle);
            writer.Write(command.aladdiumAmount);
            writer.Write(command.alageneAmount);
        }

        public static SendResourcesCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle targetFactionHandle = ReadHandle(reader);
            int aladdiumAmount = reader.ReadInt32();
            int alageneAmount = reader.ReadInt32();

            return new SendResourcesCommand(factionHandle, targetFactionHandle,
                aladdiumAmount, alageneAmount);
        }
        #endregion
        #endregion
    }
}
