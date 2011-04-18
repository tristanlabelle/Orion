using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using MoveTask = Orion.Game.Simulation.Tasks.MoveTask;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which assigns to a set of <see cref="Entity"/>s the
    /// <see cref="Task"/> to move to a destination.
    /// </summary>
    [Serializable]
    public sealed class MoveCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        private readonly Vector2 destination;
        #endregion
        
        #region Constructors
        public MoveCommand(Handle factionHandle, IEnumerable<Handle> unitHandles, Vector2 destination)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(unitHandles, "unitsToMove");

            this.unitHandles = unitHandles.Distinct().ToList().AsReadOnly();
            this.destination = destination;
        }

        public MoveCommand(Handle factionHandle, Handle unitHandle, Vector2 destination)
            : this(factionHandle, new[] { unitHandle }, destination) { }
        #endregion

        #region Properties
        public override bool IsMandatory
        {
            get { return false; }
        }

        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return unitHandles; }
        }

        public Vector2 Destination
        {
            get { return destination; }
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
                MoveTask task = new MoveTask(entity, (Point)destination);
                entity.Components.Get<TaskQueue>().Enqueue(task);
            }
        }

        public override string ToString()
        {
            return "Faction {0} moves {1} to {2}"
                .FormatInvariant(FactionHandle, unitHandles.ToCommaSeparatedValues(), destination);
        }
                
        #region Serialization
        public static void Serialize(MoveCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.unitHandles);
            writer.Write(command.destination.X);
            writer.Write(command.destination.Y);
        }

        public static MoveCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            Vector2 destination = new Vector2(x, y);
            return new MoveCommand(factionHandle, unitHandles, destination);
        }
        #endregion
        #endregion
    }
}
