using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenTK.Math;
using Orion.Collections;
using Orion.GameLogic;
using MoveTask = Orion.GameLogic.Tasks.MoveTask;

namespace Orion.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which assigns to a set of <see cref="Unit"/>s the
    /// <see cref="Task"/> to move to a destination.
    /// </summary>
    [Serializable]
    public sealed class MoveCommand : Command, IMultipleExecutingEntitiesCommand
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
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return unitHandles; }
        }

        public Vector2 Destination
        {
            get { return destination; }
        }
        #endregion

        #region Methods
        public IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles)
        {
            return new MoveCommand(FactionHandle, entityHandles, destination);
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
                MoveTask task = new MoveTask(unit, (Point)destination);
                unit.TaskQueue.OverrideWith(task);
            }
        }

        public override string ToString()
        {
            return "Faction {0} moves {1} to {2}"
                .FormatInvariant(FactionHandle, unitHandles.ToCommaSeparatedValues(), destination);
        }
                
        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, unitHandles);
            writer.Write(destination.X);
            writer.Write(destination.Y);
        }

        public static MoveCommand DeserializeSpecific(BinaryReader reader)
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
