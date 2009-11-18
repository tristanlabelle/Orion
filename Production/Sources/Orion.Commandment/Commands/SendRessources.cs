using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using System.IO;

namespace Orion.Commandment.Commands
{
    class SendRessources : Command
    { 
        #region Instance
        #region Fields
        private readonly Faction faction;
        private readonly Faction impliedOtherFaction;
        private readonly  int AladdiumAmount;
        private readonly int AlageneAmount;
        #endregion

        #region Constructors

        public SendRessources(Faction faction, Faction otherFaction, int AladdiumAmount, int AlageneAmount)
            : base(faction)
        {
            Argument.EnsureNotNull(otherFaction, "otherFaction");
            Argument.EnsureNotNull(faction, "faction");
            this.faction = faction;
            this.impliedOtherFaction = otherFaction;
            this.AladdiumAmount = AladdiumAmount;
            this.AlageneAmount = AlageneAmount;
        }
        #endregion

        #region Properties
        
        public override IEnumerable<Entity> EntitiesInvolved
        {
            get { return new List<Entity>(); }
        }

        #endregion

        #region Methods
        public override void Execute()
        {
            if(faction.AladdiumAmount >= AladdiumAmount 
                && faction.AlageneAmount >= AlageneAmount)
            {
                faction.AladdiumAmount -= AladdiumAmount;
                faction.AlageneAmount -= AlageneAmount;
                impliedOtherFaction.AladdiumAmount += AladdiumAmount;
                impliedOtherFaction.AlageneAmount += AlageneAmount;
            }
        }

        public override string ToString()
        {
            return "[{0}] send {2} of Aladdium and {3} of Alagene to {1}".FormatInvariant(faction.Name, impliedOtherFaction.Name, AladdiumAmount, AlageneAmount);
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
                writer.Write(command.SourceFaction.ID);
                writer.Write(command.impliedOtherFaction.ID);
                writer.Write(command.AladdiumAmount);
                writer.Write(command.AlageneAmount);
            }

            protected override SendRessources DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                Faction receverFaction = ReadFaction(reader,world);
                int AladdiumAmount = reader.ReadInt32();
                int AlageneAmount = reader.ReadInt32();

                return new SendRessources(sourceFaction, receverFaction,AladdiumAmount,AlageneAmount);
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
