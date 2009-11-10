using System;
using System.Collections.Generic;
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
        private readonly List<Unit> units;
        #endregion

        #region Constructor
        public Suicide(Faction faction, IEnumerable<Unit> units)
            : base(faction)
        {
            Argument.EnsureNoneNull(units, "units");
            this.units = units.ToList();
            Argument.EnsureNotNullNorEmpty(this.units, "units");
            if (units.Any(unit => unit.Faction != faction))
            {
                throw new ArgumentException(
                    "One suicided unit isn't part of the faction issuing the command.",
                    "units");
            }
        }
        #endregion

        #region Properties
        public override IEnumerable<Entity> EntitiesInvolved
        {
            get { return units.Cast<Entity>(); }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit suicider in units)
            {
                suicider.Suicide();
            }
        }

        public override string ToString()
        {
            return "[{0}] suicide".FormatInvariant(units.ToCommaSeparatedValues());
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
                writer.Write(command.units.Count());
                foreach (Unit unit in command.units)
                    writer.Write(unit.ID);
            }

            protected override Suicide DeserializeData(BinaryReader reader, World world)
            {
                Unit[] units = ReadLengthPrefixedUnitArray(reader, world);
                return new Suicide(units[0].Faction, units);
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
