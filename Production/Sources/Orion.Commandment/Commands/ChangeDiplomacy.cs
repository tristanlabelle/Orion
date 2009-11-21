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
        private readonly Handle otherFactionHandle;
        private readonly DiplomaticStance diplomaticStance;
        #endregion

        #region Constructors
        public ChangeDiplomacy(Handle factionHandle, Handle otherFactionHandle, DiplomaticStance diplomaticStance)
            : base(factionHandle)
        {
            Argument.EnsureDefined(diplomaticStance, "diplomaticStance");
            this.otherFactionHandle = otherFactionHandle;
            this.diplomaticStance = diplomaticStance;
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

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            Faction otherFaction = match.World.FindFactionFromHandle(otherFactionHandle);
            faction.SetDiplomaticStance(otherFaction, diplomaticStance);
        }

        public override string ToString()
        {
            return "[{0}] {2} to {1}".FormatInvariant(FactionHandle, diplomaticStance, otherFactionHandle);
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
                WriteHandle(writer, command.FactionHandle);
                WriteHandle(writer, command.otherFactionHandle);
                writer.Write((byte)command.diplomaticStance);
            }

            protected override ChangeDiplomacy DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                Handle otherFactionHandle = ReadHandle(reader);
                DiplomaticStance stance = (DiplomaticStance)reader.ReadByte();

                return new ChangeDiplomacy(factionHandle, otherFactionHandle, stance);
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
