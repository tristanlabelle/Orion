using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.Commands
{
    public sealed class DisembarkCommand : Command
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
        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return unitHandles; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match.World, FactionHandle)
                && unitHandles.All(handle => IsValidEntityHandle(match.World, handle));
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
        public static void Serialize(DisembarkCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.unitHandles);
        }

        public static DisembarkCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);

            return new DisembarkCommand(factionHandle, unitHandles);
        }
        #endregion
        #endregion
    }
}
