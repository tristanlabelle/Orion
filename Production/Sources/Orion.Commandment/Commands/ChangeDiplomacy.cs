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
        private readonly Faction impliedOtherFaction;
        private readonly string changeDone;
        #endregion

        #region Constructors
       
        public ChangeDiplomacy(Faction faction, Faction impliedOtherFaction, string changeDone)
            : base(faction)
        {
            Argument.EnsureNotNull(impliedOtherFaction, "newAlly");
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(changeDone, "changeDone");
            Debug.Assert(changeDone == "Ally" || changeDone == "Ennemy");
            this.faction = faction;
            this.impliedOtherFaction = impliedOtherFaction;
            this.changeDone = changeDone;
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
            if(changeDone == "Ally")faction.addAlly(impliedOtherFaction.ID);
            if (changeDone == "Ennemy") faction.addEnemy(impliedOtherFaction.ID);
        }

        public override string ToString()
        {
            return "[{0}] {2} to {1}".FormatInvariant(faction, changeDone,impliedOtherFaction);
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
                writer.Write(command.impliedOtherFaction.ID);
                writer.Write(command.changeDone);
            }

            protected override ChangeDiplomacy DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                Faction impliedOtherFaction = ReadFaction(reader, world);
                string changeDone = reader.ReadString();
                return new ChangeDiplomacy(sourceFaction, impliedOtherFaction, changeDone);
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
