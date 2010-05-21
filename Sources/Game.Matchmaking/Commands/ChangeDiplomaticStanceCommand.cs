using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes a <see cref="Faction"/> to change its
    /// diplomatic stance with regard to another <see cref="Faction"/>.
    /// </summary>
    public sealed class ChangeDiplomaticStanceCommand: Command
    {
        #region Fields
        private readonly Handle targetFactionHandle;
        private readonly DiplomaticStance diplomaticStance;
        #endregion

        #region Constructors
        public ChangeDiplomaticStanceCommand(Handle factionHandle, Handle targetFactionHandle, DiplomaticStance diplomaticStance)
            : base(factionHandle)
        {
            Argument.EnsureDefined(diplomaticStance, "diplomaticStance");
            Argument.EnsureDefined(diplomaticStance, "diplomaticStance");
            this.targetFactionHandle = targetFactionHandle;
            this.diplomaticStance = diplomaticStance;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { yield break; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            Faction otherFaction = match.World.FindFactionFromHandle(targetFactionHandle);
            faction.SetDiplomaticStance(otherFaction, diplomaticStance);
        }

        public override string ToString()
        {
            return "Faction {0} changes {1} to {2}"
                .FormatInvariant(FactionHandle, targetFactionHandle, diplomaticStance);
        }

        #region Serialization
        public static void Serialize(ChangeDiplomaticStanceCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteHandle(writer, command.targetFactionHandle);
            writer.Write((byte)command.diplomaticStance);
        }

        public static ChangeDiplomaticStanceCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle otherFactionHandle = ReadHandle(reader);
            DiplomaticStance stance = (DiplomaticStance)reader.ReadByte();
            if (!Enum.IsDefined(typeof(DiplomaticStance), stance))
                throw new InvalidCastException("{0} is not a member of DiplomaticStance".FormatInvariant(stance));

            return new ChangeDiplomaticStanceCommand(factionHandle, otherFactionHandle, stance);
        }
        #endregion
        #endregion
    }
}
