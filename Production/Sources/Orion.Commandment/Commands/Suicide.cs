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
        #region Instance
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
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Suicide"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<Suicide>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Suicide command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteLengthPrefixedHandleArray(writer, command.unitHandles);
            }

            protected override Suicide DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                var unitHandles = ReadLengthPrefixedHandleArray(reader);
                return new Suicide(factionHandle, unitHandles);
            }
            #endregion
            #endregion

            #region Static
            #region Fields
            /// <summary>
            /// A globally available static instance of this class.
            /// </summary>
            public static readonly Serializer Instance = new Serializer();
            #endregion
            #endregion
        }
        #endregion
    }
}
