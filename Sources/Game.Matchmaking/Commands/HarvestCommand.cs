﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using HarvestTask = Orion.Game.Simulation.Tasks.HarvestTask;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes the <see cref="HarvestTask"/>
    /// to be assigned to some <see cref="Unit"/>s.
    /// </summary>
    public sealed class HarvestCommand : Command, IMultipleExecutingEntitiesCommand
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> harvesterHandles;
        private readonly Handle resourceNodeHandle;
        #endregion

        #region Constructors
        public HarvestCommand(Handle factionHandle, IEnumerable<Handle> harvestersHandles, Handle resourceNodeHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(harvestersHandles, "harvestersHandles");
            this.harvesterHandles = harvestersHandles.Distinct().ToList().AsReadOnly();
            this.resourceNodeHandle = resourceNodeHandle;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return harvesterHandles; }
        }

        public Handle TargetHandle
        {
            get { return resourceNodeHandle; }
        }
        #endregion

        #region Methods
        public IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles)
        {
            return new HarvestCommand(FactionHandle, entityHandles, resourceNodeHandle);
        }

        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && harvesterHandles.All(handle => IsValidEntityHandle(match, handle))
                && IsValidEntityHandle(match, resourceNodeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            ResourceNode resourceNode = (ResourceNode)match.World.Entities.FromHandle(resourceNodeHandle);
            foreach (Handle harvesterHandle in harvesterHandles)
            {
                Unit harvester = (Unit)match.World.Entities.FromHandle(harvesterHandle);
                harvester.TaskQueue.OverrideWith(new HarvestTask(harvester, resourceNode));
            }
        }

        public override string ToString()
        {
            return "Faction {0} harvests {1} with {2}"
                .FormatInvariant(FactionHandle, resourceNodeHandle, harvesterHandles.ToCommaSeparatedValues());
        }
        
        #region Serialization
        public static void Serialize(HarvestCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.harvesterHandles);
            WriteHandle(writer, command.resourceNodeHandle);
        }

        public static HarvestCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var harvesterHandles = ReadLengthPrefixedHandleArray(reader);
            Handle resourceNodeHandle = ReadHandle(reader);
            return new HarvestCommand(factionHandle, harvesterHandles, resourceNodeHandle);
        }
        #endregion
        #endregion
    }
}
