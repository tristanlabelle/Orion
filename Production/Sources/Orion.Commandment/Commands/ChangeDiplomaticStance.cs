﻿using System;
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
        private readonly Handle otherFactionHandle;
        private readonly DiplomaticStance diplomaticStance;
        #endregion

        #region Constructors
        public ChangeDiplomaticStance(Handle factionHandle, Handle otherFactionHandle, DiplomaticStance diplomaticStance)
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
        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle);
        }

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

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteHandle(writer, otherFactionHandle);
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