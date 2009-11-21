using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using RepairTask = Orion.GameLogic.Tasks.Repair;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Unit"/>s
    /// to attack another <see cref="Unit"/>.
    /// </summary>
    public sealed class Repair : Command
    {
        #region Instance
        #region Fields
        private readonly ReadOnlyCollection<Handle> unitHandles;
        private readonly Handle targetHandle;
        #endregion

        #region Constructors
        public Repair(Handle factionHandle, IEnumerable<Handle> units, Handle targetHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(units, "units");

            this.unitHandles = units.Distinct().ToList().AsReadOnly();
            this.targetHandle = targetHandle;
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

            Unit target = (Unit)match.World.Entities.FindFromHandle(targetHandle);
            foreach (Handle unitHandle in unitHandles)
            {
                Unit unit = (Unit)match.World.Entities.FindFromHandle(unitHandle);
                unit.Task = new RepairTask(unit, target);
            }
        }

        public override string ToString()
        {
            return "[{0}] repair {1}".FormatInvariant(unitHandles.ToCommaSeparatedValues(), targetHandle);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Repair"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<Repair>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Repair command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteLengthPrefixedHandleArray(writer, command.unitHandles);
                WriteHandle(writer, command.targetHandle);
            }

            protected override Repair DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                var unitHandles = ReadLengthPrefixedHandleArray(reader);
                Handle targetHandle = ReadHandle(reader);
                return new Repair(factionHandle, unitHandles, targetHandle);
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