using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Engine;
using System.IO;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which cancels a single <see cref="Task"/> from a specific <see cref="Entity"/>.
    /// </summary>
    [Serializable]
    public sealed class CancelTaskCommand : Command
    {
        #region Fields
        private readonly Handle unitHandle;
        private readonly Handle taskHandle;
        #endregion

        #region Constructors
        public CancelTaskCommand(Handle factionHandle, Handle unitHandle, Handle taskHandle)
            : base(factionHandle)
        {
            this.unitHandle = unitHandle;
            this.taskHandle = taskHandle;
        }
        #endregion

        #region Properties
        public override bool IsMandatory
        {
            get { return true; }
        }

        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { yield return unitHandle; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && IsValidEntityHandle(match, unitHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Entity entity = match.World.Entities.FromHandle(unitHandle);

            TaskQueue taskQueue = entity.Components.Get<TaskQueue>();
            Task task = taskQueue.TryResolveTask(taskHandle);
            if (task != null) taskQueue.CancelTask(task);
        }

        public override string ToString()
        {
            return "Faction {0} cancels unit {1}'s {2} task"
                .FormatInvariant(FactionHandle, unitHandle, taskHandle);
        }

        #region Serialization
        public static void Serialize(CancelTaskCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteHandle(writer, command.unitHandle);
            WriteHandle(writer, command.taskHandle);
        }

        public static CancelTaskCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle unitHandle = ReadHandle(reader);
            Handle taskHandle = ReadHandle(reader);
            return new CancelTaskCommand(factionHandle, unitHandle, taskHandle);
        }
        #endregion
        #endregion
    }
}
