using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Engine;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Matchmaking.Commands
{
    public sealed class EmbarkCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        private readonly Handle transporterHandle;
        #endregion

        #region Constructors
        public EmbarkCommand(Handle factionHandle, IEnumerable<Handle> unitHandles, Handle transporterHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(unitHandles, "units");
            if (unitHandles.Contains(transporterHandle))
                throw new ArgumentException("A unit cannot embark in itself.");

            this.unitHandles = unitHandles.Distinct().ToList().AsReadOnly();
            this.transporterHandle = transporterHandle;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return unitHandles; }
        }

        public Handle TransporterHandle
        {
            get { return transporterHandle; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match.World, FactionHandle)
                && unitHandles.All(handle => IsValidEntityHandle(match.World, handle))
                && IsValidEntityHandle(match.World, transporterHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Unit target = (Unit)match.World.Entities.FromHandle(transporterHandle);
            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)match.World.Entities.FromHandle(unitHandle);
                unit.TaskQueue.Enqueue(new EmbarkTask(unit, target));
            }
        }

        public override string ToString()
        {
            return "Faction {0} embarks {1} in {2}"
                .FormatInvariant(FactionHandle, unitHandles.ToCommaSeparatedValues(), transporterHandle);
        }
        #region Serialization
        public static void Serialize(EmbarkCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.unitHandles);
            WriteHandle(writer, command.transporterHandle);
        }

        public static EmbarkCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            Handle targetHandle = ReadHandle(reader);
            return new EmbarkCommand(factionHandle, unitHandles, targetHandle);
        }
        #endregion
        #endregion
    }
}
