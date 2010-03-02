﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Collections;
using Orion.GameLogic;

namespace Orion.Matchmaking.Commands
{
    public sealed class DisembarkCommand : Command, IMultipleExecutingEntitiesCommand
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        #endregion

        #region Constructor
        public DisembarkCommand(Handle factionHandle, IEnumerable<Handle> unitHandles)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(unitHandles, "unitHandles");
            this.unitHandles = unitHandles.Distinct().ToList().AsReadOnly();
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return unitHandles; }
        }
        #endregion

        #region Methods
        public IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles)
        {
            return new SuicideCommand(FactionHandle, entityHandles);
        }

        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && unitHandles.All(handle => IsValidEntityHandle(world, handle));
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");
            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)match.World.Entities.FromHandle(unitHandle);
                unit.Disembark();
            }
        }

        public override string ToString()
        {
            return "Faction {0} disembarks {1}"
                .FormatInvariant(FactionHandle, unitHandles.ToCommaSeparatedValues());
        }

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, unitHandles);
        }

        public static SuicideCommand DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            return new SuicideCommand(factionHandle, unitHandles);
        }
        #endregion
        #endregion
    }
}