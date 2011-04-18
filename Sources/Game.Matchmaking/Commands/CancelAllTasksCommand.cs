using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which cancels all <see cref="Task"/>s of a set of <see cref="Entity"/>s.
    /// </summary>
    [Serializable]
    public sealed class CancelAllTasksCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        #endregion

        #region Constructors
        public CancelAllTasksCommand(Handle factionHandle, IEnumerable<Handle> unitHandles)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(unitHandles, "unitHandles");
            this.unitHandles = unitHandles.Distinct().ToList().AsReadOnly();
        }
        #endregion

        #region Properties
        public override bool IsMandatory
        {
            get { return true; }
        }

        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return unitHandles; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && unitHandles.All(handle => IsValidEntityHandle(match, handle));
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");
            foreach (Handle unitHandle in unitHandles)
            {
                Entity entity = match.World.Entities.FromHandle(unitHandle);
                entity.Components.Get<TaskQueue>().Clear();
            }
        }

        public override string ToString()
        {
            return "Faction {0} cancels {1}"
                .FormatInvariant(FactionHandle, unitHandles.ToCommaSeparatedValues());
        }

        #region Serialization
        public static void Serialize(CancelAllTasksCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.unitHandles);
        }

        public static CancelAllTasksCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            return new CancelAllTasksCommand(factionHandle, unitHandles);
        }
        #endregion
        #endregion
    }
}
