using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using System.Diagnostics;
using System.IO;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes a <see cref="Faction"/> to change its
    /// diplomatic stance with regard to another <see cref="Faction"/>.
    /// </summary>
    public sealed class ChangeDiplomaticStance: Command
    {
        #region Fields
        private readonly Handle targetFactionHandle;
        private readonly DiplomaticStance diplomaticStance;
        #endregion

        #region Constructors
        public ChangeDiplomaticStance(Handle factionHandle, Handle targetFactionHandle, DiplomaticStance diplomaticStance)
            : base(factionHandle)
        {
            Argument.EnsureDefined(diplomaticStance, "diplomaticStance");
            this.targetFactionHandle = targetFactionHandle;
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
        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle);
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
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteHandle(writer, targetFactionHandle);
            writer.Write((byte)diplomaticStance);
        }

        public static ChangeDiplomaticStance DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle otherFactionHandle = ReadHandle(reader);
            DiplomaticStance stance = (DiplomaticStance)reader.ReadByte();

            return new ChangeDiplomaticStance(factionHandle, otherFactionHandle, stance);
        }
        #endregion
        #endregion
    }
}
