using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using StandGuardTask = Orion.Game.Simulation.Tasks.StandGuardTask;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Entity"/>s
    /// to attack another <see cref="Entity"/> without ever following it.
    /// </summary>
    public sealed class StandGuardCommand : Command
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
        public override bool IsMandatory
        {
            get { return false; }
        }

        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return guardHandles; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && guardHandles.All(handle => IsValidEntityHandle(match, handle));
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            foreach (Handle guardHandle in guardHandles)
            {
                Entity guard = match.World.Entities.FromHandle(guardHandle);
                guard.Components.Get<TaskQueue>().Enqueue(new StandGuardTask(guard));
            }
        }

        public override string ToString()
        {
            return "Faction {0} units {1} stand guard."
                .FormatInvariant(FactionHandle, guardHandles.ToCommaSeparatedValues());
        }

        #region Serialization
        public static void Serialize(StandGuardCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.guardHandles);
        }

        public static StandGuardCommand Deserialize(BinaryReader reader)
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
