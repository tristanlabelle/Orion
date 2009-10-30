using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Orion.GameLogic;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which cancels the current <see cref="Task"/> of a set of <see cref="Unit"/>s.
    /// </summary>
    [Serializable]
    public sealed class Cancel : Command
    {
        #region Instance
        #region Fields
        private readonly List<Unit> units;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Cancel"/> command from the faction which
        /// created the command and a sequence of <see cref="Unit"/>s for which the
        /// current <see cref="Task"/> should be canceled.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> that created this command.</param>
        /// <param name="units">
        /// The <see cref="Unit"/>s of that <see cref="Faction"/> which <see cref="Task"/>s are to be canceled.
        /// </param>
        public Cancel(Faction faction, IEnumerable<Unit> units)
            : base(faction)
        {
            Argument.EnsureNotNullNorEmpty(units, "units");
            if (units.Any(unit => unit.Faction != base.SourceFaction))
                throw new ArgumentException("Expected all units to be from the source faction.", "units");
            
            this.units = units.Distinct().ToList();
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Unit"/>s affected by this <see cref="Command"/>.
        /// </summary>
        public IEnumerable<Unit> Units
        {
            get { return units; }
        }

        public override IEnumerable<Unit> UnitsInvolved
        {
            get
            {
                foreach(Unit unit in units)
                    yield return unit;
            }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit unit in units)
                unit.Task = null;
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Cancel"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<Cancel>
        {
            #region Instance
            #region Properties
            public override byte ID
            {
                get { return 2; }
            }
            #endregion

            #region Methods
            protected override void SerializeData(Cancel command, BinaryWriter writer)
            {
                writer.Write(command.SourceFaction.ID);
                writer.Write(command.Units.Count());
                foreach (Unit unit in command.Units)
                    writer.Write(unit.ID);
            }

            protected override Cancel DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                Unit[] units = ReadLengthPrefixedUnitArray(reader, world);
                return new Cancel(sourceFaction, units);
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
