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
        private readonly Faction sender;
        private readonly Faction receiver;
        private readonly int aladdiumAmount;
        private readonly int alageneAmount;
        #endregion

        #region Constructors
        public SendRessources(Faction sender, Faction receiver,
            int aladdiumAmount, int alageneAmount)
            : base(sender)
        {
            Argument.EnsureNotNull(sender, "sender");
            Argument.EnsureNotNull(receiver, "receiver");
            Argument.EnsurePositive(aladdiumAmount, "aladdiumAmount");
            Argument.EnsurePositive(alageneAmount, "alageneAmount");
            this.sender = sender;
            this.receiver = receiver;
            this.aladdiumAmount = aladdiumAmount;
            this.alageneAmount = alageneAmount;
        }
        #endregion

        #region Properties
        public override IEnumerable<Entity> EntitiesInvolved
        {
            get { yield break; }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            if(sender.AladdiumAmount >= aladdiumAmount 
                && sender.AlageneAmount >= alageneAmount)
            {
                sender.AladdiumAmount -= aladdiumAmount;
                sender.AlageneAmount -= alageneAmount;
                receiver.AladdiumAmount += aladdiumAmount;
                receiver.AlageneAmount += alageneAmount;
            }
        }

        public override string ToString()
        {
            return "[{0}] send {2} of Aladdium and {3} of Alagene to {1}".FormatInvariant(sender.Name, receiver.Name, aladdiumAmount, alageneAmount);
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
                writer.Write(command.receiver.ID);
                writer.Write(command.aladdiumAmount);
                writer.Write(command.alageneAmount);
            }

            protected override SendRessources DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                Faction receiverFaction = ReadFaction(reader, world);
                int aladdiumAmount = reader.ReadInt32();
                int alageneAmount = reader.ReadInt32();

                return new SendRessources(sourceFaction, receiverFaction,
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
