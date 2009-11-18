using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using System.Diagnostics;
using System.IO;

namespace Orion.Commandment.Commands
{
    class ChangeDiplomacy: Command
    {
       #region Instance
        #region Fields
        private readonly Faction faction;
        private readonly int  impliedOtherFactionID;
        private readonly DiplomaticStance diplomaticStance;
        #endregion

        #region Constructors

        public ChangeDiplomacy(Faction faction, int otherFactionID, DiplomaticStance diplomaticStance)
            : base(faction)
        {
            Argument.EnsureNotNull(otherFactionID, "otherFactionID");
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureDefined(diplomaticStance, "diplomaticStance");
            this.faction = faction;
            this.impliedOtherFactionID = otherFactionID;
            this.diplomaticStance = diplomaticStance;
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
            if (diplomaticStance == DiplomaticStance.Ally)
                faction.AddAlly(impliedOtherFactionID);
            if (diplomaticStance == DiplomaticStance.Enemy)
                faction.AddEnemy(impliedOtherFactionID);
        }

        public override string ToString()
        {
            return "[{0}] {2} to {1}".FormatInvariant(faction, diplomaticStance, impliedOtherFactionID);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Attack"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<ChangeDiplomacy>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(ChangeDiplomacy command, BinaryWriter writer)
            {
                writer.Write(command.SourceFaction.ID);
                writer.Write(command.impliedOtherFactionID);
                writer.Write((byte)command.diplomaticStance);
            }

            protected override ChangeDiplomacy DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                int impliedOtherFactionId = reader.ReadInt32();
                DiplomaticStance newStance = (DiplomaticStance) reader.ReadByte();

                return new ChangeDiplomacy(sourceFaction, impliedOtherFactionId, newStance);
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
