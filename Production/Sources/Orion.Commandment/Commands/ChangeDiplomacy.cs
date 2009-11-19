using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using System.Diagnostics;
using System.IO;

namespace Orion.Commandment.Commands
{
    public sealed class ChangeDiplomacy: Command
    {
        #region Instance
        #region Fields
        private readonly Faction faction;
        private readonly Faction otherFaction;
        private readonly DiplomaticStance diplomaticStance;
        #endregion

        #region Constructors
        public ChangeDiplomacy(Faction faction, Faction otherFaction, DiplomaticStance diplomaticStance)
            : base(faction)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(otherFaction, "otherFaction");
            Argument.EnsureDefined(diplomaticStance, "diplomaticStance");
            this.faction = faction;
            this.otherFaction = otherFaction;
            this.diplomaticStance = diplomaticStance;
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
            faction.SetDiplomaticStance(faction, diplomaticStance);
        }

        public override string ToString()
        {
            return "[{0}] {2} to {1}".FormatInvariant(faction, diplomaticStance, otherFaction);
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
                writer.Write(command.SourceFaction.Handle.Value);
                writer.Write(command.otherFaction.Handle.Value);
                writer.Write((byte)command.diplomaticStance);
            }

            protected override ChangeDiplomacy DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                Faction otherFaction = ReadFaction(reader, world);
                DiplomaticStance newStance = (DiplomaticStance)reader.ReadByte();

                return new ChangeDiplomacy(sourceFaction, otherFaction, newStance);
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
