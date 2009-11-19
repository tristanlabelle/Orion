using System;
using System.Collections.Generic;
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
        private readonly List<Unit> units;
        private readonly Unit building;
        #endregion

        #region Constructors
        public Repair(Faction faction, IEnumerable<Unit> units, Unit building)
            : base(faction)
        {
            Argument.EnsureNotNull(building, "building");
            Argument.EnsureNotNullNorEmpty(units, "units");

            this.building = building;
            this.units = units.Distinct().ToList();

            if (this.units.Any(unit => unit.Faction != base.SourceFaction))
                throw new ArgumentException("Expected all units to be from the source faction.", "units");
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of <see cref="Unit"/>s.
        /// </summary>
        public int UnitsCount
        {
            get { return units.Count; }
        }

        /// <summary>
        /// Gets the sequence of <see cref="Unit"/>s participating in this command.
        /// </summary>
        public IEnumerable<Unit> Units
        {
            get { return units; }
        }

        /// <summary>
        /// Gets the target <see cref="Unit"/> to be repaired.
        /// </summary>
        public Unit Building
        {
            get { return building; }
        }

        public override IEnumerable<Entity> EntitiesInvolved
        {
            get
            {
                foreach (Unit unit in units)
                    yield return unit;
                yield return building;
            }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit unit in units)
                unit.Task = new RepairTask(unit, building);
        }

        public override string ToString()
        {
            return "[{0}] repair {1}".FormatInvariant(units.ToCommaSeparatedValues(), building);
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
                writer.Write(command.SourceFaction.Handle.Value);
                writer.Write(command.UnitsCount);
                foreach (Unit unit in command.Units)
                    writer.Write(unit.Handle.Value);
                writer.Write(command.Building.Handle.Value);
            }

            protected override Repair DeserializeData(BinaryReader reader, World world)
            {
                Faction sourceFaction = ReadFaction(reader, world);
                Unit[] units = ReadLengthPrefixedUnitArray(reader, world);
                Unit building = ReadUnit(reader, world);
                return new Repair(sourceFaction, units, building);
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