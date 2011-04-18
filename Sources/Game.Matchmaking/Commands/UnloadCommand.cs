using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Components;
using OpenTK;

namespace Orion.Game.Matchmaking.Commands
{
    public sealed class UnloadCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        #endregion

        #region Constructors
        public UnloadCommand(Handle factionHandle, IEnumerable<Handle> units)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(units, "units");
            this.unitHandles = units.Distinct().ToList().AsReadOnly();
        }
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
                Entity transporter = match.World.Entities.FromHandle(unitHandle);
                Spatial spatial = transporter.Spatial;
                Transporter transportComponent = transporter.Components.TryGet<Transporter>();
                if (spatial == null || transporter == null) continue;

                // duplicate the Passengers enumerable because the loop will modify the actual version
                foreach (Entity passenger in transportComponent.Passengers.ToArray())
                    transportComponent.Disembark(passenger);
            }
        }

        public override string ToString()
        {
            return "Faction {0} unloads everyone with {2}"
                .FormatInvariant(FactionHandle, unitHandles.ToCommaSeparatedValues());
        }

        #region Serialization
        public static void Serialize(UnloadCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.unitHandles);
        }

        public static UnloadCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            return new UnloadCommand(factionHandle, unitHandles);
        }
        #endregion
        #endregion
    }
}