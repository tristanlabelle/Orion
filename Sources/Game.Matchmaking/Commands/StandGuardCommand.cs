﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using StandGuardTask = Orion.Game.Simulation.Tasks.StandGuardTask;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Unit"/>s
    /// to attack another <see cref="Unit"/> without ever following it.
    /// </summary>
    public sealed class StandGuardCommand : Command, IMultipleExecutingEntitiesCommand
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> guardHandles;
        #endregion

        #region Constructors
        public StandGuardCommand(Handle factionHandle, IEnumerable<Handle> guardHandles)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(guardHandles, "guardHandles");
            this.guardHandles = guardHandles.Distinct().ToList().AsReadOnly();
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return guardHandles; }
        }
        #endregion

        #region Methods
        public IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles)
        {
            return new StandGuardCommand(FactionHandle, entityHandles);
        }

        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && guardHandles.All(handle => IsValidEntityHandle(world, handle));
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            foreach (Handle guardHandle in guardHandles)
            {
                Unit guard = (Unit)match.World.Entities.FromHandle(guardHandle);
                guard.TaskQueue.OverrideWith(new StandGuardTask(guard));
            }
        }

        public override string ToString()
        {
            return "Faction {0} units {1} stand guard."
                .FormatInvariant(FactionHandle, guardHandles.ToCommaSeparatedValues());
        }

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, guardHandles);
        }

        public static StandGuardCommand DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var guardHandles = ReadLengthPrefixedHandleArray(reader);
            return new StandGuardCommand(factionHandle, guardHandles);
        }
        #endregion
        #endregion
    }
}