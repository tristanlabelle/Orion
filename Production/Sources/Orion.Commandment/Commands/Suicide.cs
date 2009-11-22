using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using System.IO;

namespace Orion.Commandment.Commands
{
    public sealed class Suicide : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        #endregion

        #region Constructor
        public Suicide(Handle factionHandle, IEnumerable<Handle> unitHandles)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(unitHandles, "unitHandles");
            this.unitHandles = unitHandles.Distinct().ToList().AsReadOnly();
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return unitHandles; }
        }
        #endregion

        #region Methods
        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");
            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)match.World.Entities.FindFromHandle(unitHandle);
                unit.Suicide();
            }
        }

        public override string ToString()
        {
            return "[{0}] suicide".FormatInvariant(unitHandles.ToCommaSeparatedValues());
        }

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, unitHandles);
        }

        public static Suicide DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var unitHandles = ReadLengthPrefixedHandleArray(reader);
            return new Suicide(factionHandle, unitHandles);
        }
        #endregion
        #endregion
    }
}
